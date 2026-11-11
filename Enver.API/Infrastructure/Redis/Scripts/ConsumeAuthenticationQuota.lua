-- enver.auth.quota / 1. Todas las keys se declaran en KEYS y comparten hash tag.
-- No hay rollback Lua: validación/plan de lectura antes de escribir; dirty persiste ante error parcial.
local MAX = 9007199254740991
local function reply(status, category, retry, now)
    return {'enver.auth.quota', '1', status, category, string.format('%.0f', retry or 0), string.format('%.0f', now or 0)}
end
local function unavailable(category, now) return reply('unavailable', category, 0, now) end
local function integer(s)
    if type(s) ~= 'string' or not string.match(s, '^%d+$') or (#s > 1 and string.sub(s, 1, 1) == '0') then return nil end
    local n = tonumber(s)
    if not n or n < 0 or n > MAX or n ~= math.floor(n) then return nil end
    return n
end
local function uuid(s)
    return type(s) == 'string' and #s == 32 and string.match(s, '^[0-9a-f]+$') and s ~= string.rep('0', 32)
end
local function token(s) return type(s) == 'string' and #s > 0 and #s <= 128 and string.match(s, '^[A-Za-z0-9_.%-]+$') end
local function opaque(s)
    return type(s) == 'string' and #s == 43 and string.match(s, '^[A-Za-z0-9_%-]+$') and
        string.find('AEIMQUYcgkosw048', string.sub(s, -1), 1, true)
end
local labels = {'register:email', 'register:origin', 'login:email', 'login:origin', 'login:origin-email', 'logout:origin', 'logout:account'}
local expected = {register = {'email', 'origin'}, login = {'email', 'origin', 'origin-email'}, logout = {'origin', 'account'}}
if #ARGV < 29 or ARGV[1] ~= 'enver.auth.quota' or ARGV[2] ~= '1' then return unavailable('invalid') end
local mode = ARGV[3]
if mode ~= 'evaluate' and mode ~= 'initialize' and mode ~= 'fence' and mode ~= 'maintain' and mode ~= 'activate' then return unavailable('invalid') end
local env = ARGV[4]
if #env == 0 or #env > 64 or not string.match(env, '^[a-z0-9_%-]+$') or not token(ARGV[5]) or not token(ARGV[6]) or not token(ARGV[7]) or
    ARGV[8] ~= '5000' or ARGV[9] ~= '67108864' or not uuid(ARGV[10]) then return unavailable('invalid') end
local generation, nextGeneration, attempt, operation = ARGV[10], ARGV[11], ARGV[12], ARGV[13]
local n, batch = integer(ARGV[14]), integer(ARGV[15])
if not n or n > 3 or not batch or batch > 3 or #KEYS ~= 2 + n + batch or #ARGV ~= 29 + 2 * n then return unavailable('invalid') end
if mode == 'evaluate' then
    if not expected[operation] or n ~= #expected[operation] or not uuid(attempt) or nextGeneration ~= '-' then return unavailable('invalid') end
elseif n ~= 0 or operation ~= '-' or attempt ~= '-' then return unavailable('invalid') end
if mode == 'fence' then
    if not uuid(nextGeneration) or nextGeneration == generation or batch ~= 0 then return unavailable('invalid') end
elseif nextGeneration ~= '-' then return unavailable('invalid') end
if (mode == 'initialize' or mode == 'activate') and batch ~= 0 then return unavailable('invalid') end
local definitions, maximumWindow, maximumLimit = {}, 0, 0
for i = 1, 7 do
    local limit, window = integer(ARGV[14 + 2 * i]), integer(ARGV[15 + 2 * i])
    if not limit or limit < 1 or limit > 2147483647 or not window or window < 1 then return unavailable('invalid') end
    definitions[labels[i]] = {limit = limit, window = window}
    maximumWindow, maximumLimit = math.max(maximumWindow, window), math.max(maximumLimit, limit)
end
local snapshotParts = {}
for i = 4, 9 do table.insert(snapshotParts, ARGV[i]) end
for i = 16, 29 do table.insert(snapshotParts, ARGV[i]) end
local snapshot = table.concat(snapshotParts, '|')
local prefix = 'enver:{' .. env .. ':auth:ratelimit}:'
if KEYS[1] ~= prefix .. 'metadata' or KEYS[2] ~= prefix .. 'registry' then return unavailable('invalid') end
local bucketPrefix = prefix .. 'bucket:'
local identities, seen = {}, {[KEYS[1]] = true, [KEYS[2]] = true}
for i = 3, #KEYS do
    local key = KEYS[i]
    if seen[key] or string.sub(key, 1, #bucketPrefix) ~= bucketPrefix then return unavailable('invalid') end
    seen[key] = true
    local member = string.sub(key, #bucketPrefix + 1)
    local op, scope, id = string.match(member, '^([a-z]+):([a-z%-]+):([A-Za-z0-9_%-]+)$')
    local definition = definitions[(op or '') .. ':' .. (scope or '')]
    if not definition or not opaque(id) then return unavailable('invalid') end
    identities[i] = {member = member, definition = definition}
    if i <= 2 + n then
        local index = i - 2
        if op ~= operation or scope ~= expected[operation][index] or ARGV[28 + 2 * index] ~= scope or ARGV[29 + 2 * index] ~= id then
            return unavailable('invalid')
        end
    end
end
local function keytype(key) return redis.call('TYPE', key).ok end
local metaType, registryType = keytype(KEYS[1]), keytype(KEYS[2])
if (metaType ~= 'none' and metaType ~= 'hash') or (registryType ~= 'none' and registryType ~= 'zset') then return unavailable('state') end
for i = 3, #KEYS do
    local t = keytype(KEYS[i])
    if t ~= 'none' and t ~= 'zset' then return unavailable('state') end
end
local meta = nil
if metaType == 'hash' then
    if redis.call('HLEN', KEYS[1]) ~= 12 or redis.call('PTTL', KEYS[1]) ~= -1 then return unavailable('state') end
    local v = redis.call('HMGET', KEYS[1], 'protocol', 'snapshot', 'generation', 'state', 'count', 'horizon', 'dirty', 'version', 'fence', 'drain', 'last', 'maximumWindow')
    if v[1] ~= '1' or v[2] ~= snapshot or v[3] ~= generation then return unavailable('state') end
    if (v[4] ~= 'ready' and v[4] ~= 'closed') or (v[7] ~= '0' and v[7] ~= '1') then return unavailable('state') end
    meta = {state = v[4], count = integer(v[5]), horizon = integer(v[6]), dirty = v[7], version = integer(v[8]),
        fence = integer(v[9]), drain = integer(v[10]), last = integer(v[11]), maximumWindow = integer(v[12])}
    if not meta.count or meta.count > 5000 or not meta.horizon or not meta.version or meta.version >= MAX or
        not meta.fence or not meta.drain or not meta.last or meta.maximumWindow ~= maximumWindow then return unavailable('state') end
end
if mode == 'initialize' then
    -- El llamador de CONTROL debe acreditar un ambiente jamás utilizado, no deducirlo de estas ausencias.
    if metaType ~= 'none' or registryType ~= 'none' then return unavailable('state') end
elseif mode ~= 'fence' and not meta then return unavailable('closed') end
if mode == 'evaluate' and (meta.state ~= 'ready' or meta.dirty ~= '0') then return unavailable('closed') end
if (mode == 'maintain' or mode == 'activate') and (meta.state ~= 'closed' or meta.dirty ~= '0') then return unavailable('closed') end

-- ÚNICA lectura de la fuente temporal distribuida. TIME es reloj de pared, no monotónico.
local time = redis.call('TIME')
local redisNow = tonumber(time[1]) * 1000000 + tonumber(time[2])
if redisNow < 0 or redisNow > MAX then return unavailable('state') end
-- Reloj lógico por namespace: un retroceso no avanza ventanas ni reduce retención.
local now = meta and math.max(redisNow, meta.last) or redisNow
if maximumWindow > MAX - now then return unavailable('state') end
local count = redis.call('ZCARD', KEYS[2])
if count > 5000 then return unavailable('state', now) end
local top = redis.call('ZRANGE', KEYS[2], -1, -1, 'WITHSCORES')
local topHorizon = 0
if #top > 0 then
    topHorizon = tonumber(top[2])
    if not topHorizon or topHorizon < 0 or topHorizon > MAX or topHorizon ~= math.floor(topHorizon) then return unavailable('state', now) end
end
if mode ~= 'fence' and meta then
    if registryType == 'none' then
        -- La ausencia solo es vencimiento acreditado si TODOS los horizontes conocidos terminaron.
        if meta.count > 0 and (meta.horizon == 0 or now < meta.horizon) then return unavailable('state', now) end
    elseif count ~= meta.count or topHorizon ~= meta.horizon or
        (meta.state == 'ready' and redis.call('PEXPIRETIME', KEYS[2]) ~= math.ceil(meta.horizon / 1000)) or
        (meta.state == 'closed' and redis.call('PTTL', KEYS[2]) ~= -1) then
        return unavailable('state', now)
    end
end
if (mode == 'maintain' or mode == 'activate') and now < meta.drain then return unavailable('drain', now) end
if mode == 'activate' and count ~= 0 then return unavailable('state', now) end

local releases, requiredStates, newCount, blockingDelay = {}, {}, 0, 0
-- Lecturas completas y plan, incluidos candidatos; todavía NINGUNA escritura.
for i = 3, #KEYS do
    local key, identity = KEYS[i], identities[i]
    local window, limit = identity.definition.window, identity.definition.limit
    local horizonValue = redis.call('ZSCORE', KEYS[2], identity.member)
    local horizon = horizonValue and tonumber(horizonValue) or nil
    if horizon and (horizon < 0 or horizon > MAX or horizon ~= math.floor(horizon)) then return unavailable('state', now) end
    local physicalCount = redis.call('ZCARD', key)
    -- Techo derivado de las siete definiciones confiables; no se permite trabajo lineal ilimitado.
    if physicalCount > maximumLimit then return unavailable('state', now) end
    local last = redis.call('ZRANGE', key, -1, -1, 'WITHSCORES')
    local lastScore = #last > 0 and tonumber(last[2]) or nil
    if lastScore and (lastScore < 0 or lastScore > now or lastScore ~= math.floor(lastScore) or window > MAX - lastScore) then return unavailable('state', now) end
    local active = lastScore and lastScore > now - window
    if active then
        if not horizon or horizon ~= lastScore + window or redis.call('PEXPIRETIME', key) ~= math.ceil(horizon / 1000) then return unavailable('state', now) end
    else
        if horizon and horizon > now then return unavailable('state', now) end
        if lastScore and not horizon and mode ~= 'maintain' then return unavailable('state', now) end
        if horizon or (mode == 'maintain' and lastScore) then table.insert(releases, {key = key, member = identity.member}) end
    end
    if i <= 2 + n then
        if redis.call('ZSCORE', key, attempt) then return unavailable('collision', now) end
        local live = redis.call('ZCOUNT', key, '(' .. string.format('%.0f', now - window), string.format('%.0f', now))
        if live >= limit then
            local expired = physicalCount - live
            local relevant = redis.call('ZRANGE', key, expired + live - limit, expired + live - limit, 'WITHSCORES')
            local delay = tonumber(relevant[2]) + window - now
            if delay <= 0 or delay > MAX then return unavailable('state', now) end
            -- Convertir el horizonte lógico a espera de pared incluye el rollback pendiente.
            -- No sumar delay + now: ese intermedio podría exceder MAX aunque el horizonte sea válido.
            blockingDelay = math.max(blockingDelay, tonumber(relevant[2]) + window - redisNow)
        end
        if not active then newCount = newCount + 1 end
        requiredStates[i] = {key = key, member = identity.member, window = window}
    end
end
if mode == 'evaluate' then
    if blockingDelay > 0 then return reply('rejected', 'quota', math.ceil(blockingDelay / 1000000), now) end
    if count - #releases + newCount > 5000 then return unavailable('capacity', now) end
end

-- FASE DE ESCRITURA. No se presupone rollback ante errores de Redis después de este punto.
local function number(value) return string.format('%.0f', value) end
local version = meta and meta.version + 1 or 1
local function metadata(state, gen, occupancy, horizon, fence, drain)
    redis.call('HSET', KEYS[1], 'protocol', '1', 'snapshot', snapshot, 'generation', gen, 'state', state,
        'count', number(occupancy), 'horizon', number(horizon), 'dirty', '1', 'version', number(version),
        'fence', number(fence), 'drain', number(drain), 'last', number(now), 'maximumWindow', number(maximumWindow))
end
if mode == 'initialize' then
    metadata('ready', generation, 0, 0, 0, 0)
    redis.call('HSET', KEYS[1], 'dirty', '0')
    return reply('initialized', 'ok', 0, now)
end
if mode == 'fence' then
    -- Serialización con todos los EVAL: los antiguos ya ejecutaron o fallarán por generación.
    metadata('closed', nextGeneration, count, topHorizon, now, now + maximumWindow)
    -- Preservar el inventario durante recuperación, incluso si un fallo dejó un bucket sin TTL.
    if count > 0 then redis.call('PERSIST', KEYS[2]) end
    redis.call('HSET', KEYS[1], 'dirty', '0')
    return reply('fenced', 'ok', 0, now)
end
redis.call('HSET', KEYS[1], 'dirty', '1')
for _, release in ipairs(releases) do
    redis.call('DEL', release.key)
    redis.call('ZREM', KEYS[2], release.member)
end
if mode == 'evaluate' then
    -- Registrar ANTES de crear buckets garantiza que un fallo parcial no deje buckets nuevos sin rastreo.
    for i = 3, 2 + n do
        local item = requiredStates[i]
        redis.call('ZADD', KEYS[2], now + item.window, item.member)
    end
    for i = 3, 2 + n do
        local item = requiredStates[i]
        redis.call('ZREMRANGEBYSCORE', item.key, '-inf', number(now - item.window))
        redis.call('ZADD', item.key, now, attempt)
        redis.call('PEXPIREAT', item.key, math.ceil((now + item.window) / 1000))
    end
end
local finalCount = redis.call('ZCARD', KEYS[2])
local finalTop = redis.call('ZRANGE', KEYS[2], -1, -1, 'WITHSCORES')
local finalHorizon = #finalTop > 0 and tonumber(finalTop[2]) or 0
if finalCount > 0 then
    if meta.state == 'closed' then redis.call('PERSIST', KEYS[2])
    else redis.call('PEXPIREAT', KEYS[2], math.ceil(finalHorizon / 1000)) end
end
local finalState = mode == 'activate' and 'ready' or meta.state
metadata(finalState, generation, finalCount, finalHorizon, meta.fence, meta.drain)
redis.call('HSET', KEYS[1], 'dirty', '0')
local status = mode == 'evaluate' and 'admitted' or (mode == 'activate' and 'activated' or 'maintained')
return reply(status, 'ok', 0, now)
