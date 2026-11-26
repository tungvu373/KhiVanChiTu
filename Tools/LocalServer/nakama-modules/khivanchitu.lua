local nk = require("nakama")

local SYSTEM_USER_ID = "00000000-0000-0000-0000-000000000000"
local COLLECTION_SOCIAL = "khivan_social"

local function ensure_leaderboards()
    local boards = {
        { id = "thien_kieu", sort = "desc", operator = "set" },
        { id = "dan_tong",   sort = "desc", operator = "set" },
        { id = "tram_yeu",   sort = "desc", operator = "set" }
    }
    for _, b in ipairs(boards) do
        local ok, err = pcall(function()
            nk.leaderboard_create(b.id, false, b.sort, b.operator, nil, { game = "KhiVanChiTu" })
        end)
        if not ok then
            nk.logger_debug("Leaderboard create (" .. b.id .. "): " .. tostring(err))
        end
    end
end

local function read_storage(collection, key, user_id)
    local objs = nk.storage_read({{ collection = collection, key = key, user_id = user_id or SYSTEM_USER_ID }})
    if objs and #objs > 0 and objs[1].value then
        return objs[1].value
    end
    return nil
end

local function write_storage(collection, key, value, user_id)
    nk.storage_write({{
        collection = collection,
        key = key,
        user_id = user_id or SYSTEM_USER_ID,
        value = value,
        permission_read = 2,
        permission_write = 0
    }})
end

local function rpc_post_world_chat(context, payload)
    local req = {}
    if payload and payload ~= "" then
        local ok, decoded = pcall(nk.json_decode, payload)
        if ok and type(decoded) == "table" then req = decoded end
    end

    local msg = tostring(req.message or "")
    if msg == "" then return nk.json_encode({ ok = false, error = "empty_message" }) end
    if #msg > 180 then msg = string.sub(msg, 1, 180) end

    local sender = tostring(req.daoName or context.username or "Đạo Hữu")
    local kind = tostring(req.kind or "chat")

    local data = read_storage(COLLECTION_SOCIAL, "world_chat", SYSTEM_USER_ID) or { items = {} }
    local items = data.items or {}
    table.insert(items, {
        id = nk.uuid_v4(),
        userId = context.user_id,
        daoName = sender,
        kind = kind,
        message = msg,
        ts = nk.time()
    })
    while #items > 50 do
        table.remove(items, 1)
    end
    write_storage(COLLECTION_SOCIAL, "world_chat", { items = items }, SYSTEM_USER_ID)
    return nk.json_encode({ ok = true, count = #items })
end

local function rpc_get_world_chat(context, payload)
    local data = read_storage(COLLECTION_SOCIAL, "world_chat", SYSTEM_USER_ID) or { items = {} }
    return nk.json_encode({ ok = true, items = data.items or {} })
end

local function dm_key(uid1, uid2)
    if tostring(uid1) < tostring(uid2) then
        return "dm_" .. tostring(uid1) .. "_" .. tostring(uid2)
    else
        return "dm_" .. tostring(uid2) .. "_" .. tostring(uid1)
    end
end

local function rpc_post_dm(context, payload)
    local ok, req = pcall(nk.json_decode, payload or "{}")
    if not ok or type(req) ~= "table" or not req.targetUserId or not req.message then
        return nk.json_encode({ ok = false, error = "invalid_dm" })
    end
    local msg = tostring(req.message)
    if msg == "" then return nk.json_encode({ ok = false, error = "empty" }) end
    if #msg > 160 then msg = string.sub(msg, 1, 160) end

    local key = dm_key(context.user_id, req.targetUserId)
    local data = read_storage(COLLECTION_SOCIAL, key, SYSTEM_USER_ID) or { items = {} }
    local items = data.items or {}
    table.insert(items, {
        fromId = context.user_id,
        fromName = tostring(req.daoName or context.username or "Đạo Hữu"),
        message = msg,
        ts = nk.time()
    })
    while #items > 30 do table.remove(items, 1) end
    write_storage(COLLECTION_SOCIAL, key, { items = items }, SYSTEM_USER_ID)
    return nk.json_encode({ ok = true, items = items })
end

local function rpc_get_dm(context, payload)
    local ok, req = pcall(nk.json_decode, payload or "{}")
    if not ok or type(req) ~= "table" or not req.targetUserId then
        return nk.json_encode({ ok = false, items = {} })
    end
    local key = dm_key(context.user_id, req.targetUserId)
    local data = read_storage(COLLECTION_SOCIAL, key, SYSTEM_USER_ID) or { items = {} }
    return nk.json_encode({ ok = true, items = data.items or {} })
end

local function rpc_assist_hophap(context, payload)
    local ok, req = pcall(nk.json_decode, payload or "{}")
    if not ok or type(req) ~= "table" or not req.targetUserId then
        return nk.json_encode({ ok = false, error = "missing_target" })
    end
    local target = tostring(req.targetUserId)
    if target == context.user_id then
        return nk.json_encode({ ok = false, error = "cannot_self_assist" })
    end
    local key = "hophap_" .. target
    local data = read_storage(COLLECTION_SOCIAL, key, SYSTEM_USER_ID) or { helpers = {}, count = 0 }
    local helpers = data.helpers or {}
    if #helpers >= 3 then
        return nk.json_encode({ ok = false, error = "max_assist_reached", count = #helpers })
    end
    for _, h in ipairs(helpers) do
        if h == context.user_id then
            return nk.json_encode({ ok = false, error = "already_assisted", count = #helpers })
        end
    end
    table.insert(helpers, context.user_id)
    write_storage(COLLECTION_SOCIAL, key, { helpers = helpers, count = #helpers }, SYSTEM_USER_ID)
    return nk.json_encode({ ok = true, count = #helpers })
end

local function rpc_get_my_hophap(context, payload)
    local key = "hophap_" .. tostring(context.user_id)
    local data = read_storage(COLLECTION_SOCIAL, key, SYSTEM_USER_ID) or { helpers = {}, count = 0 }
    return nk.json_encode({ ok = true, count = #(data.helpers or {}) })
end

local function rpc_consume_hophap(context, payload)
    local key = "hophap_" .. tostring(context.user_id)
    write_storage(COLLECTION_SOCIAL, key, { helpers = {}, count = 0 }, SYSTEM_USER_ID)
    return nk.json_encode({ ok = true })
end

ensure_leaderboards()
nk.register_rpc(rpc_post_world_chat, "rpc_post_world_chat")
nk.register_rpc(rpc_get_world_chat,  "rpc_get_world_chat")
nk.register_rpc(rpc_post_dm,         "rpc_post_dm")
nk.register_rpc(rpc_get_dm,          "rpc_get_dm")
nk.register_rpc(rpc_assist_hophap,   "rpc_assist_hophap")
nk.register_rpc(rpc_get_my_hophap,   "rpc_get_my_hophap")
nk.register_rpc(rpc_consume_hophap,  "rpc_consume_hophap")
nk.logger_info("[KhiVanChiTu] Nakama Lua module loaded: 3 leaderboards + chat/DM/HoPhap RPCs ready.")
