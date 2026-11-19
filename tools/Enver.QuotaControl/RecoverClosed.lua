-- CONTROL externo / recovery-closed / 1. Base historica UNKNOWN, no primer uso.
-- Inventario vacio y exclusion continua son precondiciones EXTERNAS acreditables.
-- No demuestra ausencia global de buckets dentro de este script.
local contract = 'enver.auth.quota.recovery-closed'
local function result(status, category, now)
    return {contract, '1', status, category, string.format('%.0f', now or 0)}
end
local snapshot = 'development|quota-development-academic-initial-v1|hmac-development-fecf274e-v1|academic-initial|5000|67108864|3|900000000|20|900000000|10|60000000|60|60000000|5|60000000|300|60000000|30|60000000'
if #KEYS ~= 2 or #ARGV ~= 2 or
    KEYS[1] ~= 'enver:{development:auth:ratelimit}:metadata' or
    KEYS[2] ~= 'enver:{development:auth:ratelimit}:registry' or
    ARGV[1] ~= snapshot then return result('aborted', 'invalid', 0) end
local generation = ARGV[2]
if type(generation) ~= 'string' or #generation ~= 32 or
    not string.match(generation, '^[0-9a-f]+$') or generation == string.rep('0', 32) then
    return result('aborted', 'invalid', 0)
end
if redis.call('TYPE', KEYS[1]).ok ~= 'none' then return result('aborted', 'metadata_exists', 0) end
if redis.call('TYPE', KEYS[2]).ok ~= 'none' then return result('aborted', 'registry_exists', 0) end
local time = redis.call('TIME')
local seconds, micros = tonumber(time[1]), tonumber(time[2])
if not seconds or not micros or seconds < 0 or micros < 0 or micros >= 1000000 or
    seconds ~= math.floor(seconds) or micros ~= math.floor(micros) then return result('aborted', 'time', 0) end
local now = seconds * 1000000 + micros
if now <= 0 or now > 9007199254740991 - 900000000 then return result('aborted', 'time', 0) end
local instant = string.format('%.0f', now)
local drain = string.format('%.0f', now + 900000000)
-- Unica mutacion, al final: no ready, cleanup, writes registry/buckets ni TTL.
redis.call('HSET', KEYS[1],
    'protocol', '1', 'snapshot', snapshot, 'generation', generation, 'state', 'closed',
    'count', '0', 'horizon', '0', 'dirty', '0', 'version', '1',
    'fence', instant, 'drain', drain, 'last', instant, 'maximumWindow', '900000000')
return result('created_closed', 'ok', now)
