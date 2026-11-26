using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace KhiVanChiTu.Remake
{
    public sealed class RemakeOnlineService : MonoBehaviour
    {
        public static RemakeOnlineService Instance { get; private set; }

        [Serializable]
        public sealed class ChatEntry
        {
            public string id;
            public string userId;
            public string daoName;
            public string kind; // "chat" or "announce"
            public string message;
        }

        [Serializable]
        public sealed class LeaderboardEntry
        {
            public string ownerId;
            public string displayName;
            public int rank;
            public long score;
            public long subscore;
            public string realmMeta;
        }

        [Serializable]
        public sealed class FriendEntry
        {
            public string userId;
            public string username;
            public string displayName;
            public int state; // 0=friend, 1=sent, 2=received
            public bool online;
        }

        [Serializable]
        public sealed class DmEntry
        {
            public string fromId;
            public string fromName;
            public string message;
        }

        public bool IsConnected { get; private set; }
        public bool IsConnecting { get; private set; }
        public string StatusText { get; private set; } = "Chưa kết nối LAN";
        public string MyUserId { get; private set; } = "";
        public string MyUsername { get; private set; } = "";
        public string LatestChatTicker { get; private set; } = "Chat Thế Giới (Chạm để mở)";

        public readonly List<ChatEntry> WorldChat = new List<ChatEntry>();
        public readonly Dictionary<string, List<LeaderboardEntry>> Leaderboards = new Dictionary<string, List<LeaderboardEntry>>();
        public readonly List<FriendEntry> Friends = new List<FriendEntry>();
        public readonly List<DmEntry> ActiveDmHistory = new List<DmEntry>();
        public string ActiveDmFriendId { get; private set; } = "";

        public event Action OnOnlineStateUpdated;

        private string _baseUrl = "http://127.0.0.1:8080";
        private string _serverKey = "";
        private string _bearerToken = "";
        private float _pollTimer;
        private float _scoreSyncTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState() => Instance = null;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _baseUrl = ResolveBaseUrl();
        }

        private void Start()
        {
            if (SaveService.Current.hasCreatedCharacter)
                ConnectIfNeeded();
        }

        private void Update()
        {
            if (SceneFlowController.Instance == null || !SceneFlowController.Instance.IsInGame) return;
            if (!SaveService.Current.hasCreatedCharacter) return;

            if (!IsConnected && !IsConnecting)
            {
                _pollTimer += Time.unscaledDeltaTime;
                if (_pollTimer >= 12f)
                {
                    _pollTimer = 0f;
                    ConnectIfNeeded();
                }
                return;
            }

            if (!IsConnected) return;

            _pollTimer += Time.unscaledDeltaTime;
            if (_pollTimer >= 4.5f)
            {
                _pollTimer = 0f;
                StartCoroutine(PollWorldChatAndHoPhapRoutine());
            }

            _scoreSyncTimer += Time.unscaledDeltaTime;
            if (_scoreSyncTimer >= 25f)
            {
                _scoreSyncTimer = 0f;
                StartCoroutine(PushLeaderboardsRoutine());
            }
        }

        public void ConnectIfNeeded()
        {
            if (IsConnecting || IsConnected) return;
            StartCoroutine(ConnectRoutine());
        }

        public void SwitchAccountProfile()
        {
            StopAllCoroutines();
            IsConnected = false;
            IsConnecting = false;
            _bearerToken = "";
            MyUserId = "";
            MyUsername = "";
            StatusText = "Chưa kết nối LAN";
            LatestChatTicker = "Chat Thế Giới (Chạm để mở)";
            WorldChat.Clear();
            Leaderboards.Clear();
            Friends.Clear();
            ActiveDmHistory.Clear();
            ActiveDmFriendId = "";
            OnOnlineStateUpdated?.Invoke();
            if (SaveService.Current.hasCreatedCharacter)
                ConnectIfNeeded();
        }

        public void SyncAllOnlineData()
        {
            if (!IsConnected)
            {
                ConnectIfNeeded();
                return;
            }
            StartCoroutine(SyncAllRoutine());
        }

        public void RefreshLeaderboard(string boardId)
        {
            if (!IsConnected) { ConnectIfNeeded(); return; }
            StartCoroutine(FetchLeaderboardRoutine(boardId));
        }

        public void SendWorldChat(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (!IsConnected)
            {
                BootOverlayController.Instance?.ShowToast("Chưa kết nối LAN Server (Tools/LocalServer)!");
                ConnectIfNeeded();
                return;
            }
            StartCoroutine(PostWorldChatRoutine("chat", message.Trim()));
        }

        public void BroadcastThienDaoAnnouncement(string announcement)
        {
            if (string.IsNullOrWhiteSpace(announcement) || !IsConnected) return;
            StartCoroutine(PostWorldChatRoutine("announce", $"[Thiên Đạo] {announcement}"));
        }

        public void AddFriendByUsernameOrId(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || !IsConnected) return;
            StartCoroutine(AddFriendRoutine(query.Trim()));
        }

        public void RemoveFriend(string uid)
        {
            if (string.IsNullOrEmpty(uid) || !IsConnected) return;
            StartCoroutine(RemoveFriendRoutine(uid));
        }

        public void OpenDmConversation(string friendUid)
        {
            ActiveDmFriendId = friendUid ?? "";
            if (!IsConnected || string.IsNullOrEmpty(ActiveDmFriendId)) return;
            StartCoroutine(FetchDmRoutine(ActiveDmFriendId));
        }

        public void SendDm(string friendUid, string msg)
        {
            if (string.IsNullOrEmpty(friendUid) || string.IsNullOrWhiteSpace(msg) || !IsConnected) return;
            StartCoroutine(PostDmRoutine(friendUid, msg.Trim()));
        }

        public void SendHoPhapAssist(string friendUid, string friendName)
        {
            if (!IsConnected || string.IsNullOrEmpty(friendUid)) return;
            StartCoroutine(SendHoPhapRoutine(friendUid, friendName));
        }

        public void ConsumeHoPhapOnServer()
        {
            if (!IsConnected) return;
            StartCoroutine(PostRpcRoutine("rpc_consume_hophap", "{}", null));
        }

        public void TryTruyenCongWithFriend(string friendName)
        {
            var save = SaveService.Current;
            long todayKey = DateTime.UtcNow.Year * 1000L + DateTime.UtcNow.DayOfYear;
            if (save.lastTruyenCongDayUtc == todayKey)
            {
                BootOverlayController.Instance?.ShowToast("Hôm nay bạn đã Truyền Công rồi! Ngày mai quay lại.");
                return;
            }
            double req = GameplayEngine.Instance != null ? GameplayEngine.Instance.GetRequiredTuViForCurrent() : 1000;
            double bonusTuVi = Math.Max(20, Math.Round(req * 0.05));
            save.tuVi = Math.Min(req, save.tuVi + bonusTuVi);
            save.lastTruyenCongDayUtc = todayKey;
            SaveService.Save();
            BootOverlayController.Instance?.ShowToast($"Truyền Công cùng {friendName}: +{CultivationTable.FormatNumber(bonusTuVi)} Tu Vi (5%)!");
            if (IsConnected) BroadcastThienDaoAnnouncement($"{GetSafeDaoName()} và {friendName} vừa cùng Truyền Công ngộ đạo!");
            OnOnlineStateUpdated?.Invoke();
        }

        private string ResolveBaseUrl()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                if (!string.IsNullOrEmpty(Application.absoluteURL))
                {
                    var uri = new Uri(Application.absoluteURL);
                    return $"{uri.Scheme}://{uri.Authority}";
                }
            }
            catch { }
#endif
            return "http://127.0.0.1:8080";
        }

        private static string GetSafeDaoName()
        {
            string n = SaveService.Current.daoName;
            return string.IsNullOrWhiteSpace(n) ? "Đạo Hữu" : n;
        }

        private string GetOrCreateDeviceId()
        {
            string profileSuffix = SaveService.ResolveProfileId();
            string prefKey = "KVCT_LAN_DEVICE_ID_" + (string.IsNullOrEmpty(profileSuffix) ? "default" : profileSuffix);
            string id = PlayerPrefs.GetString(prefKey, "");
            if (string.IsNullOrEmpty(id))
            {
                id = "kvct-" + Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(prefKey, id);
                PlayerPrefs.Save();
            }
            return id;
        }

        private IEnumerator ConnectRoutine()
        {
            IsConnecting = true;
            StatusText = "Đang kết nối LAN...";
            OnOnlineStateUpdated?.Invoke();

            // 1. Fetch Nakama server key from LocalServer /api/config
            using (var cfgReq = UnityWebRequest.Get(_baseUrl + "/api/config"))
            {
                cfgReq.timeout = 3;
                yield return cfgReq.SendWebRequest();
                if (cfgReq.result != UnityWebRequest.Result.Success)
                {
                    IsConnecting = false;
                    IsConnected = false;
                    StatusText = "Ngoại tuyến (Bật Tools/LocalServer để dùng LAN)";
                    OnOnlineStateUpdated?.Invoke();
                    yield break;
                }
                var cfg = JsonUtility.FromJson<ConfigResponse>(cfgReq.downloadHandler.text);
                _serverKey = cfg != null ? cfg.key : "";
            }

            if (string.IsNullOrEmpty(_serverKey))
            {
                IsConnecting = false;
                StatusText = "Không lấy được khóa LAN";
                OnOnlineStateUpdated?.Invoke();
                yield break;
            }

            // 2. Authenticate Device
            string devId = GetOrCreateDeviceId();
            string authBody = $"{{\"id\":\"{EscapeJson(devId)}\"}}";
            using (var authReq = new UnityWebRequest(_baseUrl + "/v2/account/authenticate/device?create=true", "POST"))
            {
                authReq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(authBody));
                authReq.downloadHandler = new DownloadHandlerBuffer();
                authReq.SetRequestHeader("Content-Type", "application/json");
                string basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(_serverKey + ":"));
                authReq.SetRequestHeader("Authorization", "Basic " + basic);
                authReq.timeout = 4;
                yield return authReq.SendWebRequest();

                if (authReq.result != UnityWebRequest.Result.Success)
                {
                    IsConnecting = false;
                    IsConnected = false;
                    StatusText = "Xác thực Nakama thất bại";
                    OnOnlineStateUpdated?.Invoke();
                    yield break;
                }
                var tok = JsonUtility.FromJson<AuthResponse>(authReq.downloadHandler.text);
                _bearerToken = tok != null ? tok.token : "";
            }

            if (string.IsNullOrEmpty(_bearerToken))
            {
                IsConnecting = false;
                yield break;
            }

            // 3. Sync Display Name & Fetch Account
            string putAcc = $"{{\"display_name\":\"{EscapeJson(GetSafeDaoName())}\"}}";
            using (var putReq = new UnityWebRequest(_baseUrl + "/v2/account", "PUT"))
            {
                putReq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(putAcc));
                putReq.downloadHandler = new DownloadHandlerBuffer();
                putReq.SetRequestHeader("Content-Type", "application/json");
                putReq.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                putReq.timeout = 3;
                yield return putReq.SendWebRequest();
            }

            using (var getAcc = UnityWebRequest.Get(_baseUrl + "/v2/account"))
            {
                getAcc.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                getAcc.timeout = 3;
                yield return getAcc.SendWebRequest();
                if (getAcc.result == UnityWebRequest.Result.Success)
                {
                    var acc = JsonUtility.FromJson<AccountWrapper>(getAcc.downloadHandler.text);
                    if (acc?.user != null)
                    {
                        MyUserId = acc.user.id;
                        MyUsername = acc.user.username;
                        SaveService.Current.nakamaUserId = MyUserId;
                    }
                }
            }

            IsConnecting = false;
            IsConnected = true;
            StatusText = $"Đã kết nối LAN ({MyUsername})";
            yield return SyncAllRoutine();
        }

        private IEnumerator SyncAllRoutine()
        {
            yield return PushLeaderboardsRoutine();
            yield return FetchLeaderboardRoutine("thien_kieu");
            yield return FetchLeaderboardRoutine("dan_tong");
            yield return FetchLeaderboardRoutine("tram_yeu");
            yield return FetchFriendsRoutine();
            yield return PollWorldChatAndHoPhapRoutine();
        }

        private IEnumerator PushLeaderboardsRoutine()
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            long power = (long)Math.Min(1_000_000_000_000.0, engine != null ? engine.GetTotalSwordAttackPower() : 10);
            long thienKieuScore = save.realmIndex * 100L + save.subStage;
            long danTongScore = save.alchemistRank * 100000L + save.alchemistXp;
            long tramYeuScore = (save.highestUnlockedLinearStage + 1L) * 1000000L + save.totalKills;

            string realmShort = CultivationTable.GetRealmShortName(save.realmIndex, save.subStage);
            string metaJson = EscapeJson($"{{\"daoName\":\"{EscapeJson(GetSafeDaoName())}\",\"realm\":\"{EscapeJson(realmShort)}\"}}");

            yield return SubmitRecordRoutine("thien_kieu", thienKieuScore, power, metaJson);
            yield return SubmitRecordRoutine("dan_tong", danTongScore, save.alchemistRank, metaJson);
            yield return SubmitRecordRoutine("tram_yeu", tramYeuScore, save.totalKills, metaJson);
        }

        private IEnumerator SubmitRecordRoutine(string boardId, long score, long subscore, string escapedMeta)
        {
            string body = $"{{\"score\":{score},\"subscore\":{subscore},\"metadata\":\"{escapedMeta}\"}}";
            using (var req = new UnityWebRequest($"{_baseUrl}/v2/leaderboard/{boardId}", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                req.timeout = 3;
                yield return req.SendWebRequest();
            }
        }

        private IEnumerator FetchLeaderboardRoutine(string boardId)
        {
            using (var req = UnityWebRequest.Get($"{_baseUrl}/v2/leaderboard/{boardId}?limit=20"))
            {
                req.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                req.timeout = 3;
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var parsed = JsonUtility.FromJson<LeaderboardResponse>(req.downloadHandler.text);
                    var list = new List<LeaderboardEntry>();
                    if (parsed?.records != null)
                    {
                        foreach (var r in parsed.records)
                        {
                            string dName = r.username;
                            string realmStr = "";
                            if (!string.IsNullOrEmpty(r.metadata))
                            {
                                var meta = JsonUtility.FromJson<RecordMeta>(r.metadata);
                                if (meta != null)
                                {
                                    if (!string.IsNullOrEmpty(meta.daoName)) dName = meta.daoName;
                                    realmStr = meta.realm;
                                }
                            }
                            long.TryParse(r.score, out long sc);
                            long.TryParse(r.subscore, out long sub);
                            int.TryParse(r.rank, out int rk);
                            list.Add(new LeaderboardEntry { ownerId = r.owner_id, displayName = dName, rank = rk, score = sc, subscore = sub, realmMeta = realmStr });
                        }
                    }
                    Leaderboards[boardId] = list;
                    OnOnlineStateUpdated?.Invoke();
                }
            }
        }

        private IEnumerator FetchFriendsRoutine()
        {
            using (var req = UnityWebRequest.Get($"{_baseUrl}/v2/friend?limit=50"))
            {
                req.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                req.timeout = 3;
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var parsed = JsonUtility.FromJson<FriendsResponse>(req.downloadHandler.text);
                    Friends.Clear();
                    if (parsed?.friends != null)
                    {
                        foreach (var f in parsed.friends)
                        {
                            if (f?.user == null) continue;
                            Friends.Add(new FriendEntry
                            {
                                userId = f.user.id,
                                username = f.user.username,
                                displayName = !string.IsNullOrEmpty(f.user.display_name) ? f.user.display_name : f.user.username,
                                state = f.state,
                                online = f.user.online
                            });
                        }
                    }
                    OnOnlineStateUpdated?.Invoke();
                }
            }
        }

        private IEnumerator AddFriendRoutine(string query)
        {
            string qs = query.Length >= 30 && query.Contains("-")
                ? "ids=" + UnityWebRequest.EscapeURL(query)
                : "usernames=" + UnityWebRequest.EscapeURL(query);
            using (var req = new UnityWebRequest($"{_baseUrl}/v2/friend?{qs}", "POST"))
            {
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                req.timeout = 3;
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    BootOverlayController.Instance?.ShowToast($"Đã gửi/chấp nhận kết bạn: {query}");
                    yield return FetchFriendsRoutine();
                }
                else
                {
                    BootOverlayController.Instance?.ShowToast("Không tìm thấy Đạo Hữu! Kiểm tra Username chính xác.");
                }
            }
        }

        private IEnumerator RemoveFriendRoutine(string uid)
        {
            using (var req = new UnityWebRequest($"{_baseUrl}/v2/friend?ids={UnityWebRequest.EscapeURL(uid)}", "DELETE"))
            {
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                req.timeout = 3;
                yield return req.SendWebRequest();
                yield return FetchFriendsRoutine();
            }
        }

        private IEnumerator PollWorldChatAndHoPhapRoutine()
        {
            yield return PostRpcRoutine("rpc_get_world_chat", "{}", json =>
            {
                var chatResp = JsonUtility.FromJson<ChatListWrapper>(json);
                if (chatResp?.items != null)
                {
                    WorldChat.Clear();
                    WorldChat.AddRange(chatResp.items);
                    if (WorldChat.Count > 0)
                    {
                        var last = WorldChat[WorldChat.Count - 1];
                        LatestChatTicker = $"{last.daoName}: {last.message}";
                    }
                }
            });

            yield return PostRpcRoutine("rpc_get_my_hophap", "{}", json =>
            {
                var hp = JsonUtility.FromJson<HoPhapResponse>(json);
                if (hp != null && hp.ok)
                    SaveService.Current.hoPhapBonusSteps = Mathf.Clamp(hp.count, 0, 3);
            });

            OnOnlineStateUpdated?.Invoke();
        }

        private IEnumerator PostWorldChatRoutine(string kind, string message)
        {
            string inner = $"{{\"daoName\":\"{EscapeJson(GetSafeDaoName())}\",\"kind\":\"{EscapeJson(kind)}\",\"message\":\"{EscapeJson(message)}\"}}";
            yield return PostRpcRoutine("rpc_post_world_chat", inner, _ => { });
            yield return PollWorldChatAndHoPhapRoutine();
        }

        private IEnumerator FetchDmRoutine(string targetUid)
        {
            string inner = $"{{\"targetUserId\":\"{EscapeJson(targetUid)}\"}}";
            yield return PostRpcRoutine("rpc_get_dm", inner, json =>
            {
                var dmResp = JsonUtility.FromJson<DmListWrapper>(json);
                ActiveDmHistory.Clear();
                if (dmResp?.items != null) ActiveDmHistory.AddRange(dmResp.items);
                OnOnlineStateUpdated?.Invoke();
            });
        }

        private IEnumerator PostDmRoutine(string targetUid, string msg)
        {
            string inner = $"{{\"targetUserId\":\"{EscapeJson(targetUid)}\",\"daoName\":\"{EscapeJson(GetSafeDaoName())}\",\"message\":\"{EscapeJson(msg)}\"}}";
            yield return PostRpcRoutine("rpc_post_dm", inner, json =>
            {
                var dmResp = JsonUtility.FromJson<DmListWrapper>(json);
                if (dmResp?.items != null)
                {
                    ActiveDmHistory.Clear();
                    ActiveDmHistory.AddRange(dmResp.items);
                    OnOnlineStateUpdated?.Invoke();
                }
            });
        }

        private IEnumerator SendHoPhapRoutine(string targetUid, string friendName)
        {
            string inner = $"{{\"targetUserId\":\"{EscapeJson(targetUid)}\"}}";
            yield return PostRpcRoutine("rpc_assist_hophap", inner, json =>
            {
                var res = JsonUtility.FromJson<HoPhapResponse>(json);
                if (res != null && res.ok)
                    BootOverlayController.Instance?.ShowToast($"Đã Hộ Pháp cho {friendName} (+1% Đột Phá, tổng +{res.count}%)!");
                else
                    BootOverlayController.Instance?.ShowToast("Đạo hữu này đã đủ +3% Hộ Pháp hoặc đã nhận hôm nay!");
            });
        }

        private IEnumerator PostRpcRoutine(string rpcId, string innerJson, Action<string> onPayload)
        {
            if (string.IsNullOrEmpty(_bearerToken)) yield break;
            // Nakama HTTP /v2/rpc expects a JSON-encoded string literal body
            string body = "\"" + EscapeJson(innerJson) + "\"";
            using (var req = new UnityWebRequest($"{_baseUrl}/v2/rpc/{rpcId}", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                req.timeout = 3;
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success && onPayload != null)
                {
                    var env = JsonUtility.FromJson<RpcEnvelope>(req.downloadHandler.text);
                    if (env != null && !string.IsNullOrEmpty(env.payload))
                        onPayload(env.payload);
                }
            }
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
        }

        [Serializable] private sealed class ConfigResponse { public string key; }
        [Serializable] private sealed class AuthResponse { public string token; public string refresh_token; }
        [Serializable] private sealed class AccountWrapper { public UserDto user; }
        [Serializable] private sealed class UserDto { public string id; public string username; public string display_name; public bool online; }
        [Serializable] private sealed class LeaderboardResponse { public RecordDto[] records; }
        [Serializable] private sealed class RecordDto { public string owner_id; public string username; public string score; public string subscore; public string rank; public string metadata; }
        [Serializable] private sealed class RecordMeta { public string daoName; public string realm; }
        [Serializable] private sealed class FriendsResponse { public FriendDto[] friends; }
        [Serializable] private sealed class FriendDto { public UserDto user; public int state; }
        [Serializable] private sealed class RpcEnvelope { public string payload; }
        [Serializable] private sealed class ChatListWrapper { public ChatEntry[] items; }
        [Serializable] private sealed class DmListWrapper { public DmEntry[] items; }
        [Serializable] private sealed class HoPhapResponse { public bool ok; public int count; public string error; }
    }
}
