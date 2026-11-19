local existingExpiry = redis.call('PEXPIRETIME', KEYS[1])
local requestedExpiry = tonumber(ARGV[2])

if existingExpiry == -2 then
    redis.call('SET', KEYS[1], ARGV[1], 'PXAT', requestedExpiry)
    return 1
end

if existingExpiry == -1 then
    return 0
end

if existingExpiry < requestedExpiry then
    redis.call('PEXPIREAT', KEYS[1], requestedExpiry, 'GT')
end

return 1
