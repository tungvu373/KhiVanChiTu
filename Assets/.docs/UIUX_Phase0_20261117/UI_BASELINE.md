# UI_BASELINE

Phase 0 — Full UI/UX Audit, 17/11/2026 (Việt Nam).

Phạm vi: toàn bộ 18 scene, 46 prefab trong Assets, UI-bearing scripts, Packages và ProjectSettings. Read-only serialized YAML audit, không sửa scene/prefab. GUID script được đối chiếu Assets và PackageCache. Số lượng là bản ghi serialized trên tất cả bản sao, không phải object đồng thời trong runtime. Demo không có Canvas/RectTransform được ghi nhận 0 UI.

Bằng chứng: Source = code/serialized hiện tại; Storyboard = ảnh tham chiếu; Risk = cần tái hiện. Static analysis không chứng minh button hoạt động, text không tràn hay save/load parity. Chưa flatten prefab overrides hoặc resolve runtime factory qua Editor; lambda/reflection/helper cần đối chiếu tại source call site.

Editor: Pipeline phát hiện đúng Unity 6000.6.4f1 nhưng port 7800 unreachable. Không có Console, hierarchy runtime hoặc Game View mới đáng tin cậy. Thử native capture không trả ảnh Unity tin cậy nên không dùng làm bằng chứng. Không chạy Play, compile, build, profiling hoặc test Android. Video gốc chưa được phát lại. Báo cáo cũ trong Docs/UI_Redesign là lịch sử, không phải QA pass hiện tại.


## 1. Baseline Git và bảo toàn

Branch đọc được: codex/android-release; HEAD 3b72406 (update). Git status/index bị Permission denied/chdir root, chưa xác minh được dirty list. Không stage/commit, đổi branch, reset/clean/stash.
Audit cũ ngày 12/11 ghi HEAD cedbda443b351aea875ea4b96685b1dc1acbc41a, khác source hiện tại. Backup cũ không được coi là snapshot source hiện tại. Bản audit này chỉ tạo bốn Markdown; scanner/intermediate ở Temp ngoài project. Chưa tạo component Phase 1.


## 2. Môi trường / số liệu

| Hạng mục | Giá trị |
|---|---|
| Unity | 6000.6.4f1 |
| URP / uGUI / InputSystem / Pipeline | 17.6.0 / 2.6.0 / 1.20.0 / 0.9.1-exp.1 |
| Scene / Prefab | 18 / 46 |
| UI-bearing C# | 162 |
| Serialized events | 21 |
| Controller reference rows | 1124 |
| RectTransform | 4832 |
| GridLayoutGroup | 57 |
| Image | 2791 |
| Button | 576 |
| HorizontalLayoutGroup | 14 |
| Text | 1584 |
| GraphicRaycaster | 5 |
| CanvasScaler | 5 |
| Canvas | 5 |
| VerticalLayoutGroup | 39 |
| Mask | 13 |
| ScrollRect | 63 |
| ContentSizeFitter | 36 |
| RectMask2D | 67 |
| LayoutElement | 180 |
| Slider | 18 |
| Toggle | 10 |
| InputField | 4 |
| Scrollbar | 8 |
| AspectRatioFitter | 1 |



## 3. Canvas / scaler / raycaster

| Source:block | ID | Hierarchy | Component | Values |
|---|---|---|---|---|
| Assets/_Recovery/0.unity:9021 | 412497779 | Canvas | GraphicRaycaster | m_Enabled=1; m_RaycastTarget= |
| Assets/_Recovery/0.unity:9038 | 412497780 | Canvas | CanvasScaler | m_UiScaleMode=1; m_ReferenceResolution={x: 1920, y: 1080}; m_MatchWidthOrHeight=0 |
| Assets/_Recovery/0.unity:9061 | 412497781 | Canvas | Canvas | m_RenderMode=0; m_SortingOrder=0; m_OverrideSorting=0 |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:56612 | 1451818555 | Canvas | GraphicRaycaster | m_Enabled=1; m_RaycastTarget= |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:56629 | 1451818556 | Canvas | CanvasScaler | m_UiScaleMode=1; m_ReferenceResolution={x: 1080, y: 1920}; m_MatchWidthOrHeight=0 |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:56652 | 1451818557 | Canvas | Canvas | m_RenderMode=0; m_SortingOrder=0; m_OverrideSorting=0 |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:139987 | 1451818555 | Canvas | GraphicRaycaster | m_Enabled=1; m_RaycastTarget= |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:140004 | 1451818556 | Canvas | CanvasScaler | m_UiScaleMode=1; m_ReferenceResolution={x: 1080, y: 1920}; m_MatchWidthOrHeight=0 |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:140027 | 1451818557 | Canvas | Canvas | m_RenderMode=0; m_SortingOrder=0; m_OverrideSorting=0 |
| Assets/Scenes/SampleScene.unity:35362 | 1451818555 | Canvas | GraphicRaycaster | m_Enabled=1; m_RaycastTarget= |
| Assets/Scenes/SampleScene.unity:35379 | 1451818556 | Canvas | CanvasScaler | m_UiScaleMode=1; m_ReferenceResolution={x: 1920, y: 1080}; m_MatchWidthOrHeight=0 |
| Assets/Scenes/SampleScene.unity:35402 | 1451818557 | Canvas | Canvas | m_RenderMode=0; m_SortingOrder=0; m_OverrideSorting=0 |
| Assets/Scenes/SampleScene_MobileWork.unity:153801 | 1451818555 | Canvas | GraphicRaycaster | m_Enabled=1; m_RaycastTarget= |
| Assets/Scenes/SampleScene_MobileWork.unity:153818 | 1451818556 | Canvas | CanvasScaler | m_UiScaleMode=1; m_ReferenceResolution={x: 1080, y: 1920}; m_MatchWidthOrHeight=0 |
| Assets/Scenes/SampleScene_MobileWork.unity:153841 | 1451818557 | Canvas | Canvas | m_RenderMode=0; m_SortingOrder=0; m_OverrideSorting=0 |



## 4. Verification matrix

| Kiểm tra | Kết quả |
|---|---|
| 1080×1920 | Chưa chụp mới; storyboard có nhưng không thay thế runtime baseline |
| 1080×2340 | Chưa kiểm chứng Game View |
| 720×1280 | Chưa kiểm chứng Game View |
| FPS / GC / batches / overdraw | Chưa đo |
| Compile / Console | Chưa chạy recompile hoặc đọc Console trực tiếp |
| Android build / device | Chưa chạy |
| Button invocation / save/load parity | Chưa runtime test; chỉ phân tích source |



## 5. Orientation / safe area

ProjectSettings: defaultScreenOrientation=4, allowedAutorotateToPortrait/PortraitUpsideDown/LandscapeRight/LandscapeLeft đều 1: cho phép autorotation ngang, cần kiểm active Android profile trước khi nghiệm thu portrait. Desktop defaultScreenWidth=1024/height=768 không phải bằng chứng độ phân giải Android. Không đổi settings.
SafeAreaFitter.Refresh hiện có clamp và cập nhật khi screen/inset đổi. Cần xác minh modal tạo trực tiếp dưới Canvas có đi qua safe content root không; có fitter không chứng minh toàn UI an toàn.


## 6. Source hash manifest trước xuất báo cáo

SHA-256 cho C#, scene, prefab, meta, asmdef trong Assets và các file Packages/ProjectSettings. Không phải ZIP backup phục hồi; art/binary/player saves chưa được backup mới. Không đọc nội dung save hay gọi PlayerPrefs.

| Path | SHA-256 |
|---|---|
| Assets/AllSkyFree.meta | ab998b0391af0075b9488a2e776936131c73d61ceeca655d23ee0f9bf8df9453 |
| Assets/AllSkyFree/Cartoon Base BlueSky.meta | b227476202d0e74ff10ad546f262d14aa2cb90d1b88c4fa5ce930680e5bf57c2 |
| Assets/AllSkyFree/Cartoon Base BlueSky/Day_BlueSky_Nothing Equirect.mat.meta | 700b59f97e3820f14611466a086900f670ac294fd3886e2294372dd1c9dda47d |
| Assets/AllSkyFree/Cartoon Base BlueSky/Day_BlueSky_Nothing.mat.meta | 09916c0aca7a62ced9f5c9a40f980051b06aca02f6c598e5879d2ef6b3c1fa4e |
| Assets/AllSkyFree/Cartoon Base BlueSky/Day_BlueSky_Nothing.unity | 83cbfc8ae77f7c9c227cbba97fd5fed23dfaa142bc11a17c29d8be048438b6bc |
| Assets/AllSkyFree/Cartoon Base BlueSky/Day_BlueSky_Nothing.unity.meta | 2a67d974a8b8ce9e842e95a87862b70a294423842ed8abeb1ed2dae90e6b8230 |
| Assets/AllSkyFree/Cartoon Base BlueSky/Day_BlueSky_NothingSettings.lighting.meta | dda70fb6fd8972ee0763862d8ccdfc89a25b6c6c2df3c8be5c07c14096f9a1af |
| Assets/AllSkyFree/Cartoon Base BlueSky/Sky_Day_BlueSky_Equirect.png.meta | fa1bbeee68206bae9bd3abd9c67595ee1dfe149c9b69a0012e87849d9735d830 |
| Assets/AllSkyFree/Cartoon Base BlueSky/Sky_Day_BlueSky_Nothing_Cam_0_Front+Z.png.meta | 4f49c3f671f17badbce16aad0f9c4c26c1271ea6f08664bf8817768cab81b857 |
| Assets/AllSkyFree/Cartoon Base BlueSky/Sky_Day_BlueSky_Nothing_Cam_1_Back-Z.png.meta | 4a22acf18b48d609224da236f1f7f01fd2e6b606e67c64c86abb81218b851832 |
| Assets/AllSkyFree/Cartoon Base BlueSky/Sky_Day_BlueSky_Nothing_Cam_2_Left+X.png.meta | 05652a7661a60a457c711a8e0bec6e4bb476c389a819cebab8456666e9649d52 |
| Assets/AllSkyFree/Cartoon Base BlueSky/Sky_Day_BlueSky_Nothing_Cam_3_Right-X.png.meta | 931263abbadcb0d5b9c5b80f351e5c96592ef7dfe118fc0ddd8c2b60e325c0fc |
| Assets/AllSkyFree/Cartoon Base BlueSky/Sky_Day_BlueSky_Nothing_Cam_4_Up+Y.png.meta | 8a9e2f319012b9c6b22fe4ed2663cba8f876511422a31b5258e7bec43085d728 |
| Assets/AllSkyFree/Cartoon Base BlueSky/Sky_Day_BlueSky_Nothing_Cam_5_Down-Y.png.meta | e37ac7d370f4b05eaeca9a371e3892a3523f8e52b7dcba0c88a449c9e865a607 |
| Assets/AllSkyFree/Cartoon Base NightSky.meta | c0d73b1d33fa09b5e79129ffd595989f301a4858cfcc02662cea294016dd1c2a |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky Equirect.mat.meta | 2f3b9cdbbe659aeacf7025d05a777526f674d5ee41e976959d0995818b74de88 |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky Equirect.png.meta | 7664f5074719d576dc06e3ec64fcb423bc527dea23e5e90344fee706715028ce |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky.mat.meta | a882d170cade1578d3cea88b31d0236df58cc9e43ad446b332ad7e2a35b103ec |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky.unity | d3a95f4af13a082affdc8480056b64c46b6bbb9a8f27586815afbc0dace5acaa |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky.unity.meta | 0f3d477e72733e85f5fc32e7cf1d0964356102de216123ff0b631dfcefb95d87 |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSkySettings.lighting.meta | 490abecb792b48424fdc0dd0cab9ce23f148494834126f7649cfadc94da18a81 |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky_Cam_0_Front+Z.png.meta | efe14cd77c82c8f325d0009cbb96e166a472b3f5df4b4fda7965420fe39d6cba |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky_Cam_1_Back-Z.png.meta | 88c574c18ec72d1ac1bb2a334656d0adafb99a089c62c6b18284f8a2b391944b |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky_Cam_2_Left+X.png.meta | f0d20bdc8a1dca2d9319794fbf195caf3402547ff9056695b4c5d6f36b2bf1a1 |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky_Cam_3_Right-X.png.meta | bb9fb4e1046e92cbd428c166997e347d63033589938aab31d84ec8c698846dbc |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky_Cam_4_Up+Y.png.meta | e410f746bb3f00c2503d41470d225e15fb33fa06b0eace89dd7736996989cc2f |
| Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky_Cam_5_Down-Y.png.meta | 45a18a254083403bc7227f36e8acbf9b11e90b875d5265bf68b5ac50e9ad8382 |
| Assets/AllSkyFree/Cold Night.meta | 9f21a1727ec05fa78a5d28c179684d6caf1d3098a1a5b3c1077ff1ec6be0ac18 |
| Assets/AllSkyFree/Cold Night/Cold Night Equirect.mat.meta | 19e06811616ad730a00134608852a8a2679a718a9c29f9dae15f3f664d98c94f |
| Assets/AllSkyFree/Cold Night/Cold Night Equirect.png.meta | 2220ea880002f7d8950557554d199cfeef0f2ac587d88579c3fdb564c4b936e4 |
| Assets/AllSkyFree/Cold Night/Cold Night.mat.meta | 842694789aa66397d3a3ed22202d78d973b48df8ed3e4440d1ebbf3b96ca4a6d |
| Assets/AllSkyFree/Cold Night/Cold Night.unity | 2b40d7140c9465cc1dccfafc384d1b8868a6605bde9eb7aa96e16d164af39f79 |
| Assets/AllSkyFree/Cold Night/Cold Night.unity.meta | ffaf244b3ccebfc6e64347968e58bacaa509d8b24f6cbd751b48b38f6b0dfc57 |
| Assets/AllSkyFree/Cold Night/Cold NightSettings.lighting.meta | 8161553ad0a22a5f622b89a9f461089f7e5b9614a80e48d446b649d795e1ab51 |
| Assets/AllSkyFree/Cold Night/Cold Night__Cam_0_Front+Z.png.meta | 9322bc5252305a0f392634d0daa426857dfde2449d8a1c6cb2bfc2ba85b09373 |
| Assets/AllSkyFree/Cold Night/Cold Night__Cam_1_Back-Z.png.meta | bb6958684027d7d3194aac9612b3960a56ffb7c5e1f1963c2eb53a2344d5184e |
| Assets/AllSkyFree/Cold Night/Cold Night__Cam_2_Left+X.png.meta | ecd7dcfa7cd1014d0293a6d0dda0f3a3d0219dcc98342f75dce2aa26b98099b1 |
| Assets/AllSkyFree/Cold Night/Cold Night__Cam_3_Right-X.png.meta | cd3e7af04e4fcaa7fa727bfa601e8e08b07954dd243b13469309fbdff08ac8cc |
| Assets/AllSkyFree/Cold Night/Cold Night__Cam_4_Up+Y.png.meta | 844925e507de4dd71f329b957c14447d8d9a43569c940fd8560d864a27eec389 |
| Assets/AllSkyFree/Cold Night/Cold Night__Cam_5_Down-Y.png.meta | 58f432c0e76b1ea0b9039d817f2784855551475d2ebd5130a5fff79a6e893425 |
| Assets/AllSkyFree/Cold Sunset.meta | 0674f3ed08ee4ede23dc0801907e12940634aaa2c5adf117d43e5a1567a67ddd |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset Equirect.mat.meta | dd336eb32b164803f8bca974ac75356b60ee6365632ed6b8dc846eb0e0ba26e4 |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset Equirect.png.meta | ad30ae317dc0caccc092ba86624a38886993ac3d9bf9aa453e21d8f6ecc59188 |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset.mat.meta | 74d26c5e0ad417bbb7e9d588b2d6b9c18939ef379b05daef49ab1635aaed7d20 |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset.unity | d0f05a36f24a1d9d189d0e5d411f747b27849b916858b236b2100ca67c76b543 |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset.unity.meta | fc2dcd8862ed31894e073d59b0229e2b961594631de991e448e9fe9d42db60c8 |
| Assets/AllSkyFree/Cold Sunset/Cold SunsetSettings.lighting.meta | 4d2e17fd18a1f8a0d1d4d8edc3f5677fbad8c09e3910747327a9083c4da826bb |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset__Cam_0_Front+Z.png.meta | 7d1f0cabf324bfc4b72d8b37b1a8eae72b9dc49b1510a414526a0949af5f6a84 |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset__Cam_1_Back-Z.png.meta | da4c06e28485f97898b8602e5cbd45edcc731bffd652dcfbb9b919e06fa8b22f |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset__Cam_2_Left+X.png.meta | 1f7c20e3543c00787d91df1d4d32b345706bdf9e10bb5a7f6961eecaf69af7eb |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset__Cam_3_Right-X.png.meta | 54342d0423fb40edc8f590cc5ff6a4d5142127c248c3529ec675c0e28a55c067 |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset__Cam_4_Up+Y.png.meta | e579959671242549b51b1f889681df304f9c04128f56adf6b1d7767375cddd03 |
| Assets/AllSkyFree/Cold Sunset/Cold Sunset__Cam_5_Down-Y.png.meta | 89459c098c991954e88e2cfc7ff6ab0002c870d353f326cde8e439cfccfd3902 |
| Assets/AllSkyFree/Deep Dusk.meta | 57c20d481e388955dc7bf7fb7761971d81c1bac18304d25aae18a7ecec902544 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk Equirect.mat.meta | 555e750bb1ba5b8ae63eb1ca5ecb8b57669c21339e6de88ac2d28caaf288a656 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk Equirect.png.meta | 7e21836a0ef64a5586dae8ea462e38b21efbcec32c76e432cd89adcd95d4aa74 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk.mat.meta | 490ead58259e13d3e557a8dd6c757de76de312cdb3346dcbbadb642b6317afb3 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk.unity | 22131fb1a3b1067aaacad144f32df4933338e51512854a1542fd944549154ad5 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk.unity.meta | e71fd9d757f0f98f8ccbe8097f5d2a0fece2229dfb8cba00aaeb33e0355fb282 |
| Assets/AllSkyFree/Deep Dusk/Deep DuskSettings.lighting.meta | 00b7c43891b3741fa62a2b606405ae0cc62317424aca8eb32594f627cf4d1024 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk__Cam_0_Front+Z.png.meta | 90d35f393bf75606488a61d5c860b91adc3ecf738bc99a510c4d110005fa3a90 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk__Cam_1_Back-Z.png.meta | 323fcb4f69cd38aeb43989129fa3806659ea25f8521b054cae2003ebb85bfa21 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk__Cam_2_Left+X.png.meta | 062fcb196fae3235a1563dda783e019933879595d5dec14295e76052d75d5809 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk__Cam_3_Right-X.png.meta | 913e9f0c9a26dc2affa31947d41d184bdd72bc3483fe5c89ef4a60512fbb46b7 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk__Cam_4_Up+Y.png.meta | a4af13a149dde04690a5a5e8dc0b47a32c88942367e2e505e5957f2ef00bd556 |
| Assets/AllSkyFree/Deep Dusk/Deep Dusk__Cam_5_Down-Y.png.meta | 2720254b881c08be3614d2a033f31dfa93fbc9e94e97e79d63700861aad2bed5 |
| Assets/AllSkyFree/Epic_BlueSunset.meta | 26356478ce807db2181b7bfd12d2a3ae665b5a190d006b494220ba8732f49ef8 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset Equirect.mat.meta | 4f24db41845041bd18cf6d12ffab23da6c9c15763bc7978502794839910124b0 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset.mat.meta | fd2e777ad615770f74a5d7ef4365994d60e8b86d002612b605d12457c690a0ec |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset.unity | 49de3a63adeec220dd4319dadbeb762c0b00edb0820fe00d59bac1fc75e9888d |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset.unity.meta | 2addcf64b8551c18a4b004eb03640c0b5ba23b93cf372e2d4230447d84e8dc58 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunsetSettings.lighting.meta | 5f316bcc326c2caa6ce1874d7496fa2f97cbf1f19ac83ecd64380d372413f506 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset_Cam_0_Front+Z.png.meta | 9d79e460983bf87729bc5b5c72a81b3e1fb052ca12e088859bea963f46ad2d07 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset_Cam_1_Back-Z.png.meta | 750460500d8adc4dd301818a6b3a4ae013c05fc7cd57734adb13d9363cecf280 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset_Cam_2_Left+X.png.meta | 993f783f6eaa07716fd26cf6a55393b176deef6f7fb5f2f9d66b72a2e54ceec2 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset_Cam_3_Right-X.png.meta | 1dbd91e4c37b4c387f0b090c2a31fb7de5ede4fd379807c02dd7f4a1935e0217 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset_Cam_4_Up+Y.png.meta | dbc22dc15a932e380c836272996c72026d8190387b2b84d502cd4e84330a7685 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset_Cam_5_Down-Y.png.meta | 55cef4ce1522a94fdc482fe4d0a143e213734ca61963a15c695e60e7bb697501 |
| Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset_EquiRect_flat.png.meta | 12e97b964386cf4bbd0b666ff3e3c4211d9a89e5991577e028aa2441da2cd08e |
| Assets/AllSkyFree/Epic_GloriousPink.meta | d822748bd295c93e5f1d1bdcd12e084477f086fadcbbffb2f99dd92034cc3a0d |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink Equirect.mat.meta | e473e7c47a164edb0498ae3a9ed0d2072bbc3396cde5bdd99d8689b2df6dd062 |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink.mat.meta | 1f100ab6bc207a8fb2245bb5731019b6063aa61def85c7583b38d714be22ea3b |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink.unity | 587e37af98c364887b164352c4590c593241f63dd652cfea2b217375bc01e6a2 |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink.unity.meta | ef10526adaed33b4b9382f9d145a63be79f9be1a19b579ca416e1ed4f4564dd1 |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPinkSettings.lighting.meta | 516f49b12b2f5990dafe6e64082f4acb1f7e4935313c200ccad1b210be1773cd |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink_Cam_0_Front+Z.png.meta | cdf436d6946fd79164983685dfe8a5044ac11148ba3cbe7341dd99f6b7e58fed |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink_Cam_1_Back-Z.png.meta | 8f70f79159baa7e0e3b9e3d21a1bb04a85df59cedfc58b6289d1da4b51a2a8bf |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink_Cam_2_Left+X.png.meta | ecff6725bfae2fde316571d0a76eee39ef4a8ee0db28133d9b9e4b853a0f4c9c |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink_Cam_3_Right-X.png.meta | 8c659ef45485aa28a29431cde30e3f21a6d7eab19428e77d5373007287ecc09e |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink_Cam_4_Up+Y.png.meta | 1d5e1f49910d918104ec5d2dd7ea15e84f8c635569a0cc1b4e8a51a512cea31d |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink_Cam_5_Down-Y.png.meta | 9d60ac47c4388daa587c42fc78cef8eda93d3a6cace2f98aac337896fb87dee2 |
| Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink_EquiRect.png.meta | df1277bba4474e5f2b07a870b15a4e0f934ac13fa978ab9ff5b612a5fbec7344 |
| Assets/AllSkyFree/Night MoonBurst.meta | 3890decf2f00c670496a12cb4d921f0f28029fdb6c4528ba4c4627b695b97281 |
| Assets/AllSkyFree/Night MoonBurst/AllSky_Night_MoonBurst Equirect.mat.meta | d4bc9b1fbd6ec0672ff9a5e48815defa7257c85d4be5cad2bf1c937f1b7b2be1 |
| Assets/AllSkyFree/Night MoonBurst/AllSky_Night_MoonBurst Equirect.png.meta | 6da6d0d906854820e709b501a7ddfefaae99fd60a331ac7af0eea192673e3c81 |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst.mat.meta | 43dda738a19b5b812c53104da6dc57d359ebe5dbc453e08dd70a9c0430875611 |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst.unity | 43f234ccdfc94f2f392544294f8c285a23a1419a4f2d474a8b2aede60b05a133 |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst.unity.meta | c2f0619e1dc85d7bcd135693602f4cad2768334f9af3b78bf9065dbbefb61e15 |
| Assets/AllSkyFree/Night MoonBurst/Night Moon BurstSettings.lighting.meta | b3ec6aec4c1295c21815deb3a4af1f0f6f66daf61650c835d4ef519d576954c3 |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst_Cam_0_Front+Z.png.meta | 41494b3efc03a53b118d5d2d2287b93d063b582326a9ff13cefebaf425f0f9a6 |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst_Cam_1_Back-Z.png.meta | 3272f4119334ddc96ed674af6cdf91f2267d0b9d7313987c0ec96ca33d9c3042 |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst_Cam_2_Left+X.png.meta | 13645375588bc7255d22d1c76a05412834f2cb1efbc3f792361d9d748e5e4b5c |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst_Cam_3_Right-X.png.meta | ad94a938ab4bb282786a746a7ed7b209ce9f568a873c9692eb4d17c281303f7c |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst_Cam_4_Up+Y.png.meta | 72b7d87a93cd972735e14b48d636c9fc9e2dec2fb89ebfcbd2e0d5aecb337244 |
| Assets/AllSkyFree/Night MoonBurst/Night Moon Burst_Cam_5_Down-Y.png.meta | dbeca15937bfcfb0708e906ee86933acffca7cb0003583fac4e12e1082877ef0 |
| Assets/AllSkyFree/Overcast Low.meta | 9117d672007ffe6e9178dd42b4e29ce88e9e0c00e22a4e96b451d208f2f62bdd |
| Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low Equirect.mat.meta | 838f00b4608d5f27044e9cb6d1484894cfe42c2aafd27312392159590ba26706 |
| Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.mat.meta | b503851fcf5437fe55a69272511410e6b7bf134bbc1044ec803b9acf77f550d7 |
| Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.png.meta | 6d24b16b78cf39a27308cabe189d492986a5a9b606381cbd09fb545395f2090e |
| Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.unity | 25acbfbdaf8e0758e29d925c799ad9d3882ad6b9bf2ac04cc69e876aab52c203 |
| Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.unity.meta | f1a724e22b6c33eb956b39db4605e0a6c1e731a72411ae7ec228edd336b88b86 |
| Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_LowSettings.lighting.meta | 515b4635734a09980c92ab864279ee1382f761b4fdf40d79cf10c08b909a6b3c |
| Assets/AllSkyFree/Overcast Low/Sky_AllSky_Overcast4_Low_Cam_0_Front+Z.png.meta | 9ff884df5da83a21705fb76f0d31a8358f7d27b94cea0e01e27745f8987481a4 |
| Assets/AllSkyFree/Overcast Low/Sky_AllSky_Overcast4_Low_Cam_1_Back-Z.png.meta | 5310ece6d2b76ac5ce67cf45f57662bcaf879c47075779f74688b1d2b17d79fc |
| Assets/AllSkyFree/Overcast Low/Sky_AllSky_Overcast4_Low_Cam_2_Left+X.png.meta | c1c794392184614fba3d525167a608cd0f400f996a8d807522a9d94598d4462f |
| Assets/AllSkyFree/Overcast Low/Sky_AllSky_Overcast4_Low_Cam_3_Right-X.png.meta | deea24904ff890276303df46fda3ea8d20c5e17e17265c0bb798d72ee40437cd |
| Assets/AllSkyFree/Overcast Low/Sky_AllSky_Overcast4_Low_Cam_4_Up+Y.png.meta | 0b138c99c541e50756b89f2a90f2e13cf8279b1e5b8e2f1ad1a1b25778ea1664 |
| Assets/AllSkyFree/Overcast Low/Sky_AllSky_Overcast4_Low_Cam_5_Down-Y.png.meta | 38a5042c14c9011cdda4cfb1f7f3f98ab688492fdda76d0d5e19ab9f47d47e50 |
| Assets/AllSkyFree/Space_AnotherPlanet.meta | 603791d7b16c15b8ce8201b7e34cc5ef5304857b294e37e24a2d4fd11a098213 |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet Equirect.mat.meta | 480aed569ea5f0e913469266331d50362bb3b370311df4875cd6be704531c48f |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet Equirect.png.meta | 861cb8c0a45e589b899cc2f64bc95f77783303bcba7ab50cf719a573d391dd34 |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet.mat.meta | 9b7abcd176f400073d06b4c80ee36e111b681b07b9e1e6ecbe2daed2973f6ba4 |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet.unity | 05ff34e840f2a8c141e3c802a0a037670d916a80abf41093a4b783aec3afd145 |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet.unity.meta | 8894a0d54aaf0579d3e0bd86f5c0746317aecbd0d37bf9a6e8041ff538ab0eb0 |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanetSettings.lighting.meta | 7d2dd3ca8f4c55ce2e279462d10665b29b82082f88e066d76a68627eeaa81c07 |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet_Cam_0_Front+Z.png.meta | 4d076aa8e640437427b8c785947eac6df7cc3c16335cd86d96276899718a17b1 |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet_Cam_1_Back-Z.png.meta | ef4b7a3615b16398e79eedfe899fe68d204ad3ce68bde1746627eba08af15a8a |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet_Cam_2_Left+X.png.meta | 335549c8d8ee80879742eb28712843684ae9514f6333cdd9e18904ff01b6dac2 |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet_Cam_3_Right-X.png.meta | f229530bacb92787da9bd14c97371402f2e94aefb96368c62a5082d06162c4ca |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet_Cam_4_Up+Y.png.meta | 1417b75fbdae2fd8fa8e0dde7532e57b576985a5ffdcd6beef992ff27b735efe |
| Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet_Cam_5_Down-Y.png.meta | f45bc02d5700b813161af9f2f87089017b4e0f65d55c572dcaae3793fb2c3921 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment.meta | dee31e61ecbed51cde577a477967acbb105560d408b8718a364422198ac3ff2f |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/AdditiveSprite.mat.meta | 922079e24fe0edfb6162a0613e3f0780b1b122d4d79b84a0a922d660716aa093 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/AllSkyFreeDemoEnvironment.prefab | f767589b3b6569bf8706cf704ea076641d118aae5aa6d2d1a3dcba2d6375af31 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/AllSkyFreeDemoEnvironment.prefab.meta | 560b8404aad5470ca43b97806f006618511dad936c753f4ef94741556c51d593 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/AllSky_DemoEnvironment.fbx.meta | 20cfc952ba68879943ae7205a4b4628ce773894b6d5e60acbfb94b2838c86a39 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/AllSky_DemoEnvironment02.fbx.meta | ac75d2064d63c53eba3a837e8e5575b26ad379b2675e0806e716298806f4e132 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/AllSky_DemoEnvironment_Center.fbx.meta | 8620e70456e2aabe9a182b23d47e91cd677ceebf2940251477479f29e3846bd5 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/AllSky_DemoEnvironment_Light.fbx.meta | 760a1b7549ba9533a27982efca2cda3b91014a924b1800c8846c4092b7304217 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials.meta | 6be1038aac1e8cca43cdbf62ced22ad4e093c55b01026cc2cfec9fdcc1229b5b |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_Building.mat.meta | 7706af581bedebe0564c5320d4cc30466609d4cc6712a481e57f670191f13677 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_FlagGreen.mat.meta | f786bfdf2efac464d896cc901b8b43183aa244ee94337020e2f24b603bf6f12c |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_Flag_Blue.mat.meta | ca60e1dd880239d3d353309de9ec6cb54a1db61aa9f886c6334f5f0e8a6bc2b5 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_Flag_Gold.mat.meta | 2c38528c5d8f3e69e3fe236de50afa800ca9011ebdd1c27eef3237e2c44d02aa |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_Flag_Red.mat.meta | 356f8abc705df0403c3cd68d584eab2bea50396b337df4c34af84ffd6aafd55e |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_Landscape.mat.meta | 1bdf3a0e609770502cbd690491a1724cf12dd6a1d1f0e82a6f210a0dccec4b94 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_LightIllum.mat.meta | 226216daf108d45c3c1411037da63c9b2771cb07b77883a610b77e9d4927ce8b |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_LightMetal.mat.meta | 0242022f7587ca94eb6e7dbaaeaf40d0bb9d0f2fc77fc58b8f6c9e0c6222b59b |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_MountainGlow.mat.meta | dc3339eb5224c43de2452243391d486ab14b7b991fe73ed5d205f775a6c0b06a |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_Rock.mat.meta | 97eab3f05100a880514c3423f3271b2ac76167a5124a57c9eac1ab767face1f0 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_TreeLeaves.mat.meta | 926603b6d322d38fe8b3b1f74115cfc2e2cf8006016aed0734a513c645cee386 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/AllSkyDemoEnv_TreeTrunk.mat.meta | 973d1eaabc1905b397067d6d0ed317c8c422dd3e342725c6ed297a17f37fb17f |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/Landscape.mat.meta | 9f05c8757fc6aa0eaca9c61b0dbf12c09fdb059f30387c78ade5fc82cd005ece |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/TreeLeaves.mat.meta | ebc1f0a7d133c936dbc3d73895e225f37d89d77ab7b737d057c3eacc257fa7de |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/TreeTrunk.mat.meta | 4be372f505bcba80abde8d3909143b0c7e9ece53fe08b9681d6d1c7928d2e1f7 |
| Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/Materials/rock.mat.meta | 93a79b7259b62f321a72e2dec9753c39138b3b5b7ccb0b8d5a13cd0ab69fe5c6 |
| Assets/AllSkyFree/_AllSkyFree_Documentation.pdf.meta | 812d0026a84a31f23d9b67a28605ce90f90db94eeb8178e1cd5384af1ab7d1a0 |
| Assets/Art.meta | 30585250652959e77d29e05bf6cc89d96a4bf239fede8ece43ea8ef2970f1ac0 |
| Assets/Art/UI.meta | 375e6464412d3eafc42e642e8cde75c53ff1f4f43f5487bfdb398e41401fc8af |
| Assets/Art/UI/Generated.meta | 753e4c2d120149c5ac76e81cd2578c29657b19e6efd906339702b0167289dca9 |
| Assets/Art/UI/Generated/ImmortalRealmPortrait.png.meta | 04e0dc562d31e52b019de2b411048af55e65fc4d59df75754fbf97d88a1b5fad |
| Assets/Art/UI/Generated/JadeGoldAtlas.png.meta | c82e4fce7f4d0040c43ee6579353f1f017331de4c1ff014b7008af12f9b937a1 |
| Assets/Art/UI/Generated/JadeV5_AlchemyDecoration.png.meta | 7994d85e81ac6444e5576cd617d202fa1214668a9cae577a3a1db638873e192f |
| Assets/Art/UI/Generated/JadeV5_CultivationHalo.png.meta | 8ac98ee50e4b45a804ffffc300a4655da6488183a2b0985d6eb67fc9ca03b774 |
| Assets/Art/UI/Generated/JadeV5_EquipmentSlot.png.meta | 3b7df9d3e76d107085f0aea3e6dcc15b77a948de588f502cadfbaffc7ee78ed1 |
| Assets/Art/UI/Generated/JadeV5_FormationDecoration.png.meta | ccf4e11452fc0d8938944a281d8e9f65e152068a3246e34cd36f26bd2414cd02 |
| Assets/Art/UI/Generated/JadeV5_GoldJadeBorder.png.meta | 395d081dd755b28fe2cdef2e2293befa6cba5ce23e36375e379c9fd4531ba970 |
| Assets/Art/UI/Generated/JadeV5_HudContainer.png.meta | 2c8508efa98864a6f349b6cf57f57f0762f4d38e3292343e5f88c06de8242a4c |
| Assets/Art/UI/Generated/JadeV5_NavigationButton.png.meta | 48cd12bbe95b7b54197862acdc816954b07d0325fcce895c392dfae6d7670016 |
| Assets/Art/UI/Generated/JadeV5_PanelBackground.png.meta | 0c3c2909bfa46eac71df2848dd5f3362d0361e1b1f694c56aac8a7c5dca5090c |
| Assets/Art/UI/Generated/JadeV5_PopupFrame.png.meta | 4641b0607ab404a05ba8637b08b47b005191b76843238d454412541f2b3d38bb |
| Assets/Art/UI/Generated/JadeV5_RarityBlue.png.meta | 489bd8e6de90295cd6b0ebbf4ad672dfe4e2528a1f0df7dae7248a3b0b817c7b |
| Assets/Art/UI/Generated/JadeV5_RarityGold.png.meta | 7b48b357aff84e32babdec0f2f57e350b1a01942480f050c7e2f216646d801ea |
| Assets/Art/UI/Generated/JadeV5_RarityGreen.png.meta | 72f06aac8788ae70f6cd840dd8bf94df29fdb753324338cabd3d289ad9241d9c |
| Assets/Art/UI/Generated/JadeV5_RarityNeutral.png.meta | c13928fdde711bdacbaa4ea61cb1759dd625435cb86f4b0f69f5e174973424e0 |
| Assets/Art/UI/Generated/JadeV5_RarityOrange.png.meta | d5e912b3fd1a930e0d42be3b74ce22c7b33b9c2cf84a3f4592eebd1e7d175cba |
| Assets/Art/UI/Generated/JadeV5_RarityViolet.png.meta | 0af106dc170b00336b8be058c78574d8fd3c24dabf2ad8f55b42f532eacbb274 |
| Assets/Art/UI/Generated/JadeV5_ResourceCoDuyen.png.meta | 385becf76ea12ed42fb1062039a3719d4dc49c264597567932a008a41e24eb55 |
| Assets/Art/UI/Generated/JadeV5_ResourceMedicineDust.png.meta | 97d3a7a378622692945b18c327cf171017d2a6c0b58c542cf7f88300dec12024 |
| Assets/Art/UI/Generated/JadeV5_ResourceOre.png.meta | a6faf9912081e3d6c63309a84f0fd6d07bd663498052760adf8111d42fbb8349 |
| Assets/Art/UI/Generated/JadeV5_ResourceSpiritStone.png.meta | 22f9eb00ddaa0aa81ceda76ad64319b15a8e731c3777b0fdff6b32d91fee3907 |
| Assets/Art/UI/Generated/JadeV5_ResourceTrack.png.meta | e0ace933b053d4427dc9b1c5f3c85013b99560fd3620dcebf173d5cecbfb53b6 |
| Assets/Art/UI/Generated/JadeV5_RewardFrame.png.meta | d7f017d99476d41aadfff054056584d5ebe58cfccf6db1e4cb9a694f11db901c |
| Assets/Art/UI/Generated/JadeV5_SectionDivider.png.meta | a4ec2a3ac5bb63ac0fa9df1ba41e0e497ef6c0d1eedbe1405d38525da69bb751 |
| Assets/Art/UI/Generated/JadeV5_SelectionFrame.png.meta | 6a630cde1e5974ad856e1ca02cfe46c6291f7c384cd739c841cadbcebc47773c |
| Assets/Art/UI/Generated/JadeV5_TabBackground.png.meta | ae6a68283bed6b7266c0f6b4f7821de100b99039519c5d99d3bb7288b5b38bd8 |
| Assets/Art/UI/Generated/JadeV5_UpgradeButton.png.meta | c1678f427ae41154dd59969a70625cade6138e951f59a12f58130b9dc6a3d3d5 |
| Assets/Art/UI/Generated/Prefabs.meta | be6cf72cda04de74c50aa80505a5c0e41ae33721957ef6fbf9eaeeae4a48b322 |
| Assets/Art/UI/Generated/Prefabs/JadePanel.prefab | 2538e1ef992f21eadf5f982c69c70703cb6f4a00c0150e0af7a541d1e42c7261 |
| Assets/Art/UI/Generated/Prefabs/JadePanel.prefab.meta | d969dac7312cab6f046d33bcd2031cf8569460d6d12fb9b3d7d9ae42176be1d4 |
| Assets/Art/UI/Generated/Prefabs/JadePrimaryButton.prefab | acf967a76c381317479eaba7fd129d93d5acd108063d950a8eaded7e7315c046 |
| Assets/Art/UI/Generated/Prefabs/JadePrimaryButton.prefab.meta | 52deb51212d58a89353dba4ee91474a1ea21032300a820956f320ebce6c0071e |
| Assets/Art/UI/Generated/Prefabs/JadeResourceBar.prefab | 5c4bf81b5a2204abdfdb0a0134c71c5895e914053279aa261787a3c9aba14e69 |
| Assets/Art/UI/Generated/Prefabs/JadeResourceBar.prefab.meta | 49366733bbc405eaf59ae24b6f5912cf51340b506361a52db0801eb3030321b7 |
| Assets/Art/UI/Generated/Prefabs/JadeSecondaryButton.prefab | 0fa4ab7ae9f05368f7bf77e2602450064c67ca2d20224eca1d0189c1a25c0044 |
| Assets/Art/UI/Generated/Prefabs/JadeSecondaryButton.prefab.meta | 05a3a741b61d1647a6e486ae63e2ebd2f2176d1b318a92b05097112cf598425c |
| Assets/Art/UI/Generated/Prefabs/JadeSlot.prefab | a59138fab9a0cba98131fb12fe717bf954f0ec40a17a49d50fe847a87ed355c3 |
| Assets/Art/UI/Generated/Prefabs/JadeSlot.prefab.meta | 691eb2ed079e8f0800c393af17da67f50a38172a1b1c27e486ee5b9f85b1ff0e |
| Assets/Art/UI/Generated/Prefabs/JadeWindow.prefab | cfba4a0c1353358e2bdce85d14348e1aba74373a106089d0649fa1ccb953e584 |
| Assets/Art/UI/Generated/Prefabs/JadeWindow.prefab.meta | 95910559613026b3ad733f7966dcc6bc98180ecaf72e44fc3df2f1f7b5602a09 |
| Assets/Art/UI/Generated/asset_manifest.json.meta | 1faf02d5052beb041653af4b1a24d3c956a7e792d88dcf19078853560b450726 |
| Assets/Holotna.meta | 33777fadbf968aee55cec86b432d9cf38f3a176467b760330f06da7522c60791 |
| Assets/Holotna/Mountain.meta | f55f41016d27188973ca84ed2bb3c2260e0639609ccbc27ed5248aa321b32b3a |
| Assets/Holotna/Mountain/Materials.meta | 24852f0adbaddf078a3b13f518e9b0b7b6842e720f735afc6cdc445007f16c61 |
| Assets/Holotna/Mountain/Materials/Bridge01.mat.meta | 1e8f22216ce827af1293426743539ecc046e2eb4050a4737a8eac60b7bcec4d4 |
| Assets/Holotna/Mountain/Materials/Bush01.mat.meta | 9c7fbeb278ffd83d4dcc3b8f8c027791ec5b72aea1151fd062ff9644888ef92e |
| Assets/Holotna/Mountain/Materials/Flower01A.mat.meta | a1cd84d2deeb59728f86ac625a4ff03ea04af233a0eeb19e756d09ed761fa726 |
| Assets/Holotna/Mountain/Materials/Flower01B.mat.meta | c648821ed893cead47be20048b914888cb727ad97baed936010c877b3b0ec6aa |
| Assets/Holotna/Mountain/Materials/Grass01A.mat.meta | b144a524ae1232a54711fee745725aa08facb7e095031ca83f4f4f3ce2f2ae64 |
| Assets/Holotna/Mountain/Materials/Grass01B.mat.meta | f8531b702ae543d6bed3fa3378402128bd210ddda357a9d8ba2ad4f950e304b0 |
| Assets/Holotna/Mountain/Materials/Grass01C.mat.meta | 9af1429aa7f6e504b34ddba6a5af2da14ce52e6ecd813b93c73cebb56b56ef5c |
| Assets/Holotna/Mountain/Materials/Leaf01A.mat.meta | fbfa4298446b9ff58282d2ca08f38a80d1019ad32bc8411e7a1d6148f7c7b395 |
| Assets/Holotna/Mountain/Materials/Leaf01B.mat.meta | e74ab573272e8491d50542f73e27992c624d46ed75e44454199e65ff8e1720d5 |
| Assets/Holotna/Mountain/Materials/Rock01A.mat.meta | 9a1f1b9086fd79eca8979b12948e2d5459276cbcab353a5cb8a6cfdf54fa2f60 |
| Assets/Holotna/Mountain/Materials/Rock01B.mat.meta | 55df23dd09e070dd6c0db20812d6911d33bc3afdbde01d6ab012b7b7ed9cfc15 |
| Assets/Holotna/Mountain/Materials/Rock01C.mat.meta | b6eb91d2fe09e6345008bae4b29aa7bd0d5428024479853b310936f80fd2b4dc |
| Assets/Holotna/Mountain/Materials/Trunk01A.mat.meta | 50e82aca82de5fb8657fe5f9e52b4b576de2fec333991525d451960691030210 |
| Assets/Holotna/Mountain/Materials/Trunk01B.mat.meta | e0bf3ac01bb5841efc2d17069760338a9a68bb115c84ce08ef05528d3f2d2b8c |
| Assets/Holotna/Mountain/Misc.meta | 8e3c1018ff13a12be23d71c9fd7c37f64831449fcba525d8a9b605c82a0e41f1 |
| Assets/Holotna/Mountain/Misc/Ground01.png.meta | eac65ca300843b58f9844174b9da56df5749c4919034663996b3feddebe4e31b |
| Assets/Holotna/Mountain/Misc/Ground01.terrainlayer.meta | fb8adee17ecbeef1194d62f8a942e8ab6779e68536863007a5136a3e6e5535bd |
| Assets/Holotna/Mountain/Misc/Ground02.png.meta | a0cb1af88e7eff1b82bd5145c6acd2710c2634a6f61bc4281466cc30419c3170 |
| Assets/Holotna/Mountain/Misc/Ground02.terrainlayer.meta | d09c728de2eb0d96b5c70531b5cc7fb40f91ebd918ab157d3f453f686a5f440b |
| Assets/Holotna/Mountain/Misc/Ground03.png.meta | efe4a017cef17f47e81933a62df4c8198c47c23c6946e406fb534d3e784835cc |
| Assets/Holotna/Mountain/Misc/Ground03.terrainlayer.meta | 635efe2b06420cf5b85954eb461565bd3e2f6ae1993388b5be8ae3638758dfdf |
| Assets/Holotna/Mountain/Misc/NewLayer.terrainlayer.meta | c0d35b253903b5e2a9deaac41c05234e458c8a130d2dc83d73eb4a0b11f2418f |
| Assets/Holotna/Mountain/Misc/Skybox.hdr.meta | a480c6f8325a424d12a2b143c4d0c6edefb003bdfe668df4bee90b769eee43cb |
| Assets/Holotna/Mountain/Misc/Skybox.mat.meta | 6e246e995f36f47d05100a9c2644c55055249a8c225b00441f07661d157f86b4 |
| Assets/Holotna/Mountain/Misc/Terrain01.asset.meta | 2f115460d9bb549dfad58ebd76cecb8b7bd78010489f68ea89216b25db7a22f2 |
| Assets/Holotna/Mountain/Misc/Terrain02.asset.meta | 78650eaca3e9ea886ad51088013f06aeb1308dd691cad8fcf674a19524f9815e |
| Assets/Holotna/Mountain/Misc/Terrain03.asset.meta | 989f8a552d22fec342404de4dbf2388a551503906348f6a59fd255f544d23ba0 |
| Assets/Holotna/Mountain/Misc/Terrain04.asset.meta | 7c1b4bb10ec7980d8f708fad57846e8339bac6df22086fffb3e385daf93f31f2 |
| Assets/Holotna/Mountain/Misc/Terrain05.asset.meta | 3427db853128ad26b8669d2e984297778e0adf1b2d2bbf46c3223524171a1d21 |
| Assets/Holotna/Mountain/Misc/Terrain06.asset.meta | 55981dbf3d5ba023eae164dff8732fce12f5b00cdbf4569ce83eb30b22e221ab |
| Assets/Holotna/Mountain/Misc/Terrain07.asset.meta | f55faafdc0040e2471790ec2248dee73e76f44c353fc91340de6205039cb0dc9 |
| Assets/Holotna/Mountain/Misc/Terrain08.asset.meta | dffe41f64ce659a4c7a10f11130635085b69841973465023b33a9bd0ed3cfb73 |
| Assets/Holotna/Mountain/Misc/Terrain09.asset.meta | 95c8fa5fd1e5738b5b994b2c7efc8375545638410804b136c3fb4733d023f690 |
| Assets/Holotna/Mountain/Misc/TerrainData_d8d389c0-2d8b-412d-981c-84c45add7612.asset.meta | 21b572c39e98992deb8caa4c1710c7cdc3970f5aa082307e077b802a12f5990d |
| Assets/Holotna/Mountain/Models.meta | c0b7771b0c8fb75075dd75963ae306c02094ba2c270ead40592269ff5695dc4a |
| Assets/Holotna/Mountain/Models/Bridge01.fbx.meta | d9f083a2cdc15e9b5715e3011b0c1f5a74b54aafbdb8ebdfcb25ed8818cb43ad |
| Assets/Holotna/Mountain/Models/Bush01.fbx.meta | b32ee6da52f3e22b85a588eb4130a75c127d5cb8273241d01c56038589003986 |
| Assets/Holotna/Mountain/Models/Flower01.fbx.meta | eab8d924e814475df9975ceb8d10a96ad99aedf3ed99c3780b5ed81fa90d4603 |
| Assets/Holotna/Mountain/Models/Flowers01.fbx.meta | 12ba828965690b6601159efd9776481d7fa188e11ba01072d24e4c17cf788f2d |
| Assets/Holotna/Mountain/Models/Grass01.fbx.meta | 8e34ed75688482cade98740c7a028437ddc61f83369c5d5c0c0c48a86e91c6a5 |
| Assets/Holotna/Mountain/Models/Mountain01.fbx.meta | 2b695879a13c98b10d0a877715798d0b4c57b5c919820f8761822555e58e61fb |
| Assets/Holotna/Mountain/Models/Pebbles01.fbx.meta | 75e7d3770fd59a04b940f8e1ef2fdd42579c208a5a0b4644d0b7b7af72434b2c |
| Assets/Holotna/Mountain/Models/Rock01.fbx.meta | 6e2af2ddba86cfb19fbbdf3849db8d0f5d367195e0bca39ac82ea015a2d62812 |
| Assets/Holotna/Mountain/Models/Rock02.fbx.meta | 5ce203b800112174608105ef07a26c7734c187fab48397f9c1be0aec41a2c6f6 |
| Assets/Holotna/Mountain/Models/Tree01.fbx.meta | d5579b499cf4baf2d312e4af185c09913addb60a07826e767c2f87726714458b |
| Assets/Holotna/Mountain/Prefabs.meta | 78bb84b6910631f5133d3ce8440350240d185e6b9d52cba92e9422af6da4d2d6 |
| Assets/Holotna/Mountain/Prefabs/Bridge01.prefab | 4dba62672f21fcdef2e4dd31ea49df5691facf68df0457c1c05e88f593718945 |
| Assets/Holotna/Mountain/Prefabs/Bridge01.prefab.meta | 35c2c8ab7198d5df59e7f07b1eca7cef2be546eb693fd7e05b3be3fe07c54455 |
| Assets/Holotna/Mountain/Prefabs/Bush01.prefab | b7219b697e2e4c7cac4fc16e745dbf7e09b2b5dd3782f74eef78565f55c18b66 |
| Assets/Holotna/Mountain/Prefabs/Bush01.prefab.meta | 27666f78e78ac070f074a7e40176121a52586607a29cc71b249c850f139d4638 |
| Assets/Holotna/Mountain/Prefabs/Flower01A.prefab | ce8061712163bf0e9fe4669f335956cb02222bcf01d6d5e19eb304c54e2d7655 |
| Assets/Holotna/Mountain/Prefabs/Flower01A.prefab.meta | 460f960fe0a5bae271c8fc2f22596ab87a3064a07749ed41860bd0cd0b037ea5 |
| Assets/Holotna/Mountain/Prefabs/Flower01B.prefab | cf9150445580f0d169414a940313becae9e86168ec2a14672c2739a08be57054 |
| Assets/Holotna/Mountain/Prefabs/Flower01B.prefab.meta | 19597d75d6a4a64b8648a0ad9e39b7e6a2e62b594e09927ddffb65f2b789af32 |
| Assets/Holotna/Mountain/Prefabs/Flowers01A.prefab | f73407d519a6087fc43c0ec6dc22a07285bb2d2519e021b944a2ff290b4c5331 |
| Assets/Holotna/Mountain/Prefabs/Flowers01A.prefab.meta | b620c7ad79a685ae0360cd4647b555ba2134ae9da2fa20086b5fc1ad5240b83c |
| Assets/Holotna/Mountain/Prefabs/Flowers01B.prefab | f2930b6ec12ac2030e2c698abe9ec3b1dd9cd05612ffad040a60cc501ee97032 |
| Assets/Holotna/Mountain/Prefabs/Flowers01B.prefab.meta | 0df316b18bd2fe13633e01de744fda9402ccf09e36b441c7f1a9228af7e9878c |
| Assets/Holotna/Mountain/Prefabs/Grass01A.prefab | 326d1a7c6160506c786c0ed8881d0dab0348ee979d8aee3b12b42c5b4de9a9bd |
| Assets/Holotna/Mountain/Prefabs/Grass01A.prefab.meta | 137501fe8b1da80ccf530bb3140d68334dcd38a8bf13eaf12bb26d6669c5b0b7 |
| Assets/Holotna/Mountain/Prefabs/Grass01B.prefab | fc89fc66b81eb03496af7b262093fdbef6ea350d65e26bfc67cc5fe04ae45465 |
| Assets/Holotna/Mountain/Prefabs/Grass01B.prefab.meta | e4ebfa2a6beebcf29a33caf9986bed17296d0acb8bff2eb9bd797fbeac947311 |
| Assets/Holotna/Mountain/Prefabs/Grass01C.prefab | 8ceb39060acb8c4ab8c53fe751c2832bf5c600005ef996ca0f5a03a2915a51c0 |
| Assets/Holotna/Mountain/Prefabs/Grass01C.prefab.meta | 5d54acf0624183681260a90f78ecaae30494415c815eba3d46fdf2ca33303d84 |
| Assets/Holotna/Mountain/Prefabs/Mountain01.prefab | 6981093057b956133c5527e1a5e9a77488bcee9507ec5a0a3451136316ea5b5e |
| Assets/Holotna/Mountain/Prefabs/Mountain01.prefab.meta | 05cdd5edad5081f27241b0ebdba67b570dc5cff6f897ab8d4f34b8daf3b184d8 |
| Assets/Holotna/Mountain/Prefabs/Pebbles01.prefab | 520d3930deac5370feb08d9134e3f40841f56623b2b8c8e7550871426422b85b |
| Assets/Holotna/Mountain/Prefabs/Pebbles01.prefab.meta | acd015c1dc28686c8b18326cf328299408db4ad7cb748832086ead042bb6504c |
| Assets/Holotna/Mountain/Prefabs/Rock01.prefab | 4a6604d5106fa72c58b95c19c7ddc5bfd23ada0d1b609f95c7599ee92cb0458d |
| Assets/Holotna/Mountain/Prefabs/Rock01.prefab.meta | d18dd3789cb435a346dbce1948e135242890dc3fbae05c646e2758a2d2b6c492 |
| Assets/Holotna/Mountain/Prefabs/Rock02.prefab | c540b63643adf3453f283c9d90d18c761f9f0c8d6cca77d30ba77cf9c4bd22b4 |
| Assets/Holotna/Mountain/Prefabs/Rock02.prefab.meta | c160b04d4d99c235c503d0391b60a29a227116d29fec4e3ceffac1ee3f86f820 |
| Assets/Holotna/Mountain/Prefabs/Tree01A.prefab | 96a277ce197926209c7f827e7bdb349d5bfb2d70187ae285b25e6a5281ea4c57 |
| Assets/Holotna/Mountain/Prefabs/Tree01A.prefab.meta | 16dbfaa3e7f31f3ee4c94e0776ed3f26aa04e50fd4f286c45484971ce7279217 |
| Assets/Holotna/Mountain/Prefabs/Tree01B.prefab | 72cb565e72079b1cfccfba76502c8105e5f43a9ad2d57b783a17651f645fa460 |
| Assets/Holotna/Mountain/Prefabs/Tree01B.prefab.meta | f4e944a9842dc4dc135a1960ee87a12b33fbdc315e65b6899b666c8d7491efbb |
| Assets/Holotna/Mountain/Scenes.meta | d643050b40737a7de2d77b91b5fbea1ea416180c7662162ca3a476711976341e |
| Assets/Holotna/Mountain/Scenes/Demo.meta | c328822fa7f9e64de8717f0ddf652a0248ac8199e3276b7b9b95c9e9fef42632 |
| Assets/Holotna/Mountain/Scenes/Demo.unity | 4fd8b36702d97ca3fe250761e46ae243cd9d0dd2bae22adcfa4ea953470a1f14 |
| Assets/Holotna/Mountain/Scenes/Demo.unity.meta | ffa4e73c089f9de0693d05f2eebd8cb7f93a91d4f189e3d1faeadc57bc739f29 |
| Assets/Holotna/Mountain/Scenes/Demo/LightingData.asset.meta | c37258df51f11181a919772b7b9ef187fb99611f5acf580dd13d3fb7b90cf7e9 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-0_comp_dir.png.meta | d3a3335b066e5d4b114d5e53113004c6163980f75a0c076a4ffba085df94d8fa |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-0_comp_light.exr.meta | 957277a5149bfa99169511739268d064b0755c84db48911bbc8165884c9727b7 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-0_comp_shadowmask.png.meta | 16b8083e650f3faa3ca9d4ad328d243e3bd772a6983e39330bfd24b41e41abf3 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-10_comp_dir.png.meta | 6940206a7f939d11cce690da89d1fbd9fc217adbfe3aa27451fb56dc094e5313 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-10_comp_light.exr.meta | 854574c85b72a4221b2c999998b6dd91122b1ee0b20acad070ec32cc5f70fb70 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-10_comp_shadowmask.png.meta | e0d87f78f1209187fa91c1d966f686db88528d541da7b3f1142156229f696238 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-11_comp_dir.png.meta | b9e047e48d167004f3b77aa7fd6a2a5fb2cd706feb11749cd9c2bf15082f8fdb |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-11_comp_light.exr.meta | 04d830e7c4f87827fc81b5f9852f687d722f7e63511a657a0992ec5e2c3c1068 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-11_comp_shadowmask.png.meta | 437e10d5bd2e0aee2bf6943e7f9fc8b79b4eb47b8380d236ac76013666abe60e |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-12_comp_dir.png.meta | 7031ffd93f706cb89f2ea89a774fa9f982be2c054fb482b98985a5dc0d5306c4 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-12_comp_light.exr.meta | 3f62da7dbf3f3da4e74b76cd48cffba229e2f5a592c8284be3c9ba1571ebfaf2 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-12_comp_shadowmask.png.meta | a2bca5290731a60b229d60dcf02843560fa28e180d6fef27007a410974d7f1f1 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-13_comp_dir.png.meta | 78de881c077b3d1322f3243d61ed3e9b97b3d2b4863cf47028d8e96fe0d48cc3 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-13_comp_light.exr.meta | 309fc84fbf4ce9a8940dd2715d59391b809276d3b301123a20c1e73219da3f2a |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-13_comp_shadowmask.png.meta | 03b1f6de0eef74737e28fb32e081f059f366acd5216af2cd4f97bbc462fba561 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-14_comp_dir.png.meta | 8b0b5eccbe307d90906ab0e4a4c488f76e73f6c3f3a2f84ceee97299bb45af3a |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-14_comp_light.exr.meta | a1e2eeeafb7941967a37e48234cd6760c1113c2a0b58a68325438d82ac79ba2d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-14_comp_shadowmask.png.meta | 288fb56a3b657e14b6e24754b5c28a0833760b772873b4f3397deb7917a0e6f3 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-15_comp_dir.png.meta | a5ee9bb9a0c7081a76f2393cc2b29dc58ed06502045ce2c007658503163c0fe5 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-15_comp_light.exr.meta | 5ab4f8371b767032c2ce20d66d4fbb2e8a6c16ead4c26b64075a08edbf0eb186 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-15_comp_shadowmask.png.meta | ebcbf1f25c6781c81f3ebc395e9652d1b03d1f11f91a781ba98fe7831bc1700c |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-16_comp_dir.png.meta | 4ca4405befa297fe528b31ef0121d0899ff63d41d690215340a74d030666cd4c |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-16_comp_light.exr.meta | 4c6cf472accbd9ca284012fc0a175aee03556a13c427bf32c337701031eb2afa |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-16_comp_shadowmask.png.meta | a1e68b45ca0682ee6259883603dcd1e17e12f70e88ea01b1349280e42009bc48 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-17_comp_dir.png.meta | 6a814b477e30edaa925f0967ec80455c07ccce69734bcc4f4be3115bc36f30d9 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-17_comp_light.exr.meta | f596ded58e0509a25c88f7526229bde85b1e33a794241126a66ce25cef1a8f05 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-17_comp_shadowmask.png.meta | d93fc8f0a370ed01762caf7c0ab63947bb8452c83a146dd439066c2d28287d25 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-18_comp_dir.png.meta | 7cf9a8794b8083a3f612bff8e4f5acd2916bab62b1e98f9f2fe78fc35197af92 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-18_comp_light.exr.meta | 9a929cee503d955c8f2e8414d3bb9ecb44b0a048b4ca53f523d8b363d6cbde27 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-18_comp_shadowmask.png.meta | f87b77f148b57b61f291081bd366b0b5d3423b042e5a028043b9452d4977a9d1 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-19_comp_dir.png.meta | 235fc98818b586945cb34c42963bcbe9f0511afafc6113310c77dacc850f7a49 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-19_comp_light.exr.meta | 328570a8f7eb77d267bf77e187cdc24f5ac9b64e5538f616836c5f38e8a899f2 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-19_comp_shadowmask.png.meta | a7ca83000689694b4cc7cdc6e9f78baec0eca7d988d3f2fccbe9d93d3d818923 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-1_comp_dir.png.meta | 6dc8a23ed85e3f895de430facee34462491eadc224c8952c7328d001456bf7fc |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-1_comp_light.exr.meta | 7398a91134f2c3109056f77d7478eb6df2ad1fcc328f653d5353834094e93165 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-1_comp_shadowmask.png.meta | fd61f8ac39a00ff8621e2112f646acd7f30d8fd5070d940f6a46a5aef347bfdd |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-20_comp_dir.png.meta | 8d05f8297e36f30ee0038a071ae79432b0877274acd53fc609fed5732bcc50fc |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-20_comp_light.exr.meta | 049fa0aa0f48808ca548f10a26edddee715a857a2f5401f669ff6fb1d6ff6059 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-20_comp_shadowmask.png.meta | 61506f0ffbd9411f48dfc5a6dca69f301f9d95ffbf7e49ca40a530562bbc8146 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-21_comp_dir.png.meta | 0080c7db18444d61fe372ba087ca7b1cf48b515663aed1cf0c2de37229a9bcea |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-21_comp_light.exr.meta | b7978debe32a08521c78b9d3b047f72d7c4bc8058eae4270f0565c88953722de |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-21_comp_shadowmask.png.meta | 7a6fbcf0863298be03dc863f3d0f5a7df556113cb0e15e7be06185ff8a3c7629 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-22_comp_dir.png.meta | a45c2ac456829f5cee02023ca5172bd5d9b4eb0242a8672c679be3646ba2fd54 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-22_comp_light.exr.meta | abebe6ad7fceb521dfe44f25035f61734fdcacb892133f35b1e508b33404a0ff |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-22_comp_shadowmask.png.meta | fdceb8b933238ea98a98db0069afd231c6877dd68e33dc4aedfa9bab8739991c |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-23_comp_dir.png.meta | 108ecd50224175883d24e7c8fa8446e442202f04246c15e210ee00b274a33eb0 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-23_comp_light.exr.meta | 54ac420624ef70e461597e11d47aeb3bb44385db7b0551070d62825c21f76eb7 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-23_comp_shadowmask.png.meta | 3ad26128b9e11c3b14d93a88149c4a54cd8ac6a337221a0f37a1707df0a4ea14 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-24_comp_dir.png.meta | 857df9d0ab9bf29730e97675b519d928dc8e1215101e29405d137df18c7f796f |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-24_comp_light.exr.meta | 39747fbc4aab0352f5ff17111f6da75681fcf4187d56813db473d4496f3cab3c |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-24_comp_shadowmask.png.meta | 073f6314c256a540f3015cf9583e14a64e0c7ff586feb75cfadde6d1deb03dfb |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-25_comp_dir.png.meta | 9d8a0f58e2e9d2af395b4c3d7563c41543a219c2be15180b318aa6f726ba99da |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-25_comp_light.exr.meta | 9c8792ed8604d43808a6ee9376fa01bc16505666c8b1438a0bd52ac928c94419 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-25_comp_shadowmask.png.meta | c08f4a072fb0653751d2d931ec43bd1697069654496e67ff34b895161ce0bc26 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-26_comp_dir.png.meta | 8425db763c50edb8dac944da4071c91bb79e56a866112f42e86bf391e71cf329 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-26_comp_light.exr.meta | 8578705a398fee2b06f86bf1c8bcdd4f8c69313af468a08f3082133babedbf9d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-26_comp_shadowmask.png.meta | f3f29009605c212a5a3736caf4824517163729c7edbab15cb53c957fa58507da |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-27_comp_dir.png.meta | 853720ee509d9fe3d892ced10fa3dc995e7d6d63044c5ea1778b1e14e3c596ca |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-27_comp_light.exr.meta | f8afb602ee70ac51826990b3e3eacd837455efe5457f2df5537540a3b87f01b3 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-27_comp_shadowmask.png.meta | 3c6ba98f2796fc555894a9086b7dcfc8a13b00e0deba034d9d3f64c0071b8b88 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-28_comp_dir.png.meta | 06f8244bf77b92b183c12ef4b9cfe03a528f9821f06f792e56710259ab2282ec |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-28_comp_light.exr.meta | ae0659b2ad0fd871d2f2ed8f98b0b94cef36f766bc77df59fddf6ff18199d7f5 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-28_comp_shadowmask.png.meta | 2ab404c58575800e1f013d8e04d0c4657ce9d6ed0a3d5308492c2ceeee84c14e |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-29_comp_dir.png.meta | cca62f136634791c6efdfa4db902156b4bda803b00939bf7ddcdb56183a16af7 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-29_comp_light.exr.meta | 23ae4a59c856e307316de2521e15c257c3fbf8116cf890ee0760cce6113d6615 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-29_comp_shadowmask.png.meta | bbb0b839ba113d25061ddb8e8d14ca4b2eaa6f9d0a0c42a46d06dbd006026e15 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-2_comp_dir.png.meta | 7926fa58d3a77a24cd98770baef192b7868882b500ec9f9e26aae547cea5062c |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-2_comp_light.exr.meta | def54eb5e42aa3731a23057c429abe868bff7eeaf372d87c354750b91f010fde |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-2_comp_shadowmask.png.meta | c7ebb11e24deb6b5db787216c22a03a1b47d1c038eab17511c2afcc9ae81530b |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-30_comp_dir.png.meta | 86af97b1a433931c00d690219480eb6321284d4972dbca516e375f3160592603 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-30_comp_light.exr.meta | 849227af90393f9c28dda218057bccb3e49d020878130ff3a2606d1d303679ce |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-30_comp_shadowmask.png.meta | 4c5bc9e028292026d766c39aaac55dfc4299cec0e4217f5347271413f16af25b |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-31_comp_dir.png.meta | 21f64876cb48c9a259fa4e088c7ed97a1b5de8a28d07d1454e0046c694bffc24 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-31_comp_light.exr.meta | 6d62fba681053f26aae381e9410a53246e971ec4c0205b7c3e60a1e613346fe1 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-31_comp_shadowmask.png.meta | c9034f6042dcfee0454d5dc3d56a797a8ede2692cee9be9666d4fbbd179f62b2 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-32_comp_dir.png.meta | 0ec37524e1eb112ab090f5da5d095747f3ceda33df57bf9f9a0001ba213906e0 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-32_comp_light.exr.meta | e25fb956e36461dbf88b7083e99b3dc0ba27648a325e1365c22d7d7c4109dd6d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-32_comp_shadowmask.png.meta | 5e31d16a8eb48f0ca327a91d078ec0f07208237ac069bf353400f517db970be7 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-33_comp_dir.png.meta | ad147f485f370619ef8dcd454fe65b8b23fd9e80fd6534d051e9641d2ac4ed13 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-33_comp_light.exr.meta | bd77d1e3c8abf673d25fcbad181d6d322087adf328f93bcc53b0afa01f47fc25 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-33_comp_shadowmask.png.meta | cb9b32b7f01e05c9cabc138b129320109871b48344465409a944c01a66ab3c63 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-34_comp_dir.png.meta | 5228d368dc0d08b9a04e9d3fa8efcf004f451f4e46ca20789512d52de45ccbd8 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-34_comp_light.exr.meta | 8eb93066ca34a104b8f39bc6fdc5d081e2cbc26c06269ddb920b967648a8ae3f |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-34_comp_shadowmask.png.meta | 0a8d0b8aab4bced1320fb4257b3799f867f0c4237c8a84555d3df96ff77bfe3a |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-35_comp_dir.png.meta | 553e82b424b40c15da69b044c586f2b4ae3549cdbaa0d3b3866200337ed4afc4 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-35_comp_light.exr.meta | 86c7518c4ac0f7f81cb9faf0f044ac05d668b6c9546d3d610d0a63ece5198005 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-35_comp_shadowmask.png.meta | 538bff7f1e797f8ea722090980bec23154a54c9c972624bda6aca9c6eba71dcc |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-36_comp_dir.png.meta | 60678559819f1dad2c97bf1518e9fced7986baec6403decea96d9d2cad69a202 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-36_comp_light.exr.meta | 0b04d598cc870102f53ed339307bb76a2a786f7165f3eb226f2b510f772a5f10 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-36_comp_shadowmask.png.meta | a38beb77dd498d3b7b8217026436a7312bc930fd43c20acf8d1bc42bf41c5258 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-37_comp_dir.png.meta | aed2d74b9e50c4852bec65f4372ac3365f049d0e972d673ef67b6f1929ef5c87 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-37_comp_light.exr.meta | a508ffc875f09e6bfd15dd7d249343146c62e9e866922fc164cdab47c0020998 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-37_comp_shadowmask.png.meta | 37102993e03ecc0c6e562b2d0b807a8df117a264784cca3c2101841d5d807f0d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-38_comp_dir.png.meta | ddf14a3934773917abc724011212b0f659d3dd57f06e13eb8d4333a90966acc9 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-38_comp_light.exr.meta | a9f51286ade8e32a8c5c5a7c5f2f9e92c6127068aac7bd1f1ed2ecdf6e827db4 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-38_comp_shadowmask.png.meta | f12bb9be63bb9003f57b9a49ee3c348dd1e2f135397e2a9d069bbc3cf9b25c9d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-39_comp_dir.png.meta | d958596c51f0153cd78bb2cfe62d66039ff237661fbdd0f2308dcd0716f5b15e |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-39_comp_light.exr.meta | 05e43f71a7902dd6c376f510bcc626451a5d86e2cd8a46ef3bbe8ab1b145ee5b |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-39_comp_shadowmask.png.meta | 039dc245aed5d6a85fb08484ea2d71c87f7b0267d8d526deebcf7cc05e724c20 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-3_comp_dir.png.meta | 04af42ad9b529530faf031369f543e406d2976cd2c0b50ce7738a85e21664f55 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-3_comp_light.exr.meta | edf7724d101c1c0079c0de26c8ca9e0f5f7083045bda63d5e2dc7032764e9554 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-3_comp_shadowmask.png.meta | 8f7e02f22932e1f1df39e13646327d07022355b467bd7c10a2f2021eea9ab970 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-40_comp_dir.png.meta | 8c617301df8d638687914100fd3f45a3eb0aa430f8a8b5a56cb6cbbaf6c231d2 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-40_comp_light.exr.meta | edbbecf1d38dc88ce1c10b85a9f3ee55ceedb40e61da065d10adba457240712a |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-40_comp_shadowmask.png.meta | b51ec63ff365312d4b693d74328d9ad413816dae4b1fbafcc98b301fd3234201 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-41_comp_dir.png.meta | b8eaa748829f0bdd8f1e4574288fd5ec6c842394c9e10b2e3aaefb620306d272 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-41_comp_light.exr.meta | 7d19002f8673b4f76c614bd8707732e9bb424286b5d73df85f7179cbe3ef432f |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-41_comp_shadowmask.png.meta | a757c0e9e3e4592e29a8c4a5a454b1bac0659316dc27ae58cfc6e85deacf27ce |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-42_comp_dir.png.meta | 3bb874bf94d7b9ea1c816537df2246936422aa1f5f79da4920bae7ed1e19331b |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-42_comp_light.exr.meta | 235b63f13fb873c0915e8ba0829c1d55a12bf8d7c6eab0bec72b56689c75983c |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-42_comp_shadowmask.png.meta | 38a545c8488ea64fc352d4fd563dadfa494c8e85dd52fda0bf9e2433ad5d5313 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-43_comp_dir.png.meta | f3cb54a3f07a1c5b920f6b9642d66be29971a050ba54c0380609fecc473564c8 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-43_comp_light.exr.meta | ee18e344d6fcaa8c17a9751983d29498a4f560508538864eb4c6b74aa13e5f92 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-43_comp_shadowmask.png.meta | f9e3091c9491555f6926e4e2cce2504ddf05cde6d74d5573c40f07e52a409899 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-44_comp_dir.png.meta | 565bfc13a6844a928be62bdca37b3aefd2a0046d2394bb41cf1a3349dc9cf5f8 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-44_comp_light.exr.meta | 5cd98bdbc17a4255ca8ccee86636b1ce2c532e88dd78e12d54c1a8a22daa65e4 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-44_comp_shadowmask.png.meta | 99ff4196c65ab4c0deb6baf53a870af54064d9c9f0b2ed1a462ebd6c1bca4163 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-45_comp_dir.png.meta | 32308e7cde9a5f6cc7f2e1bddae665c889f907c12240efa523d9c1248edf37c6 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-45_comp_light.exr.meta | 93cf85f9233c88adb593e7739f7b298df2608b4d70ecdd17e5e6e7295ec7fa9d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-45_comp_shadowmask.png.meta | 8ba0c39055805600d202e0ec6efbebe00b92bdf3e3ad24d81442e66a6fad59ae |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-46_comp_dir.png.meta | 17e094f7a3ce8e0672b2f34a0a184de0a9c87224ae88f32de11e18955a0b2620 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-46_comp_light.exr.meta | c11c3729ce0c5ff3e8f6d93adc91d416701a074c47ab9895850b13a5c8638dd0 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-46_comp_shadowmask.png.meta | b583413c56a2e2018a343299f34228b4cc7e3d3d7a8aad47a0c2b518fe745089 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-47_comp_dir.png.meta | d2fe05fdbf761febebff263778e9e21d9ab7791188b6b9a40b4f2e089ad1f9c7 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-47_comp_light.exr.meta | 9a5f46b87ea6caf8219c72560e246ef1f7a3f8e3391973579dcc9344c335220f |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-47_comp_shadowmask.png.meta | 5e85b510cf5c5402b6b330686990feafdcde2d2069fe29df6029e3b0872f0363 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-48_comp_dir.png.meta | 29a537967ea76553f1913167d6158c409b0c73a5dfec173786f16229cca35d18 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-48_comp_light.exr.meta | b3e89cc75d8c076fbb84dd3f80cf1901201411d5bfcb76a8135f0d91b29c180a |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-48_comp_shadowmask.png.meta | c763d52325bf3f72946bddc372d6db136385fa3ae68ce8330b3a2a4cb9034ab0 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-49_comp_dir.png.meta | c3cb261a1bca34640b8d2406c55168df4dd6aa22f3c9a5a22cae4044c0ef62b8 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-49_comp_light.exr.meta | 187a4528742c49d01eb5d6c30f7975f5e3c2313c3fc477eed4aebe1c9d2bdd87 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-49_comp_shadowmask.png.meta | d2c1a4717e04e8325ef5f509807c201cc45625da1066a23af11548b506d6e07c |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-4_comp_dir.png.meta | 6685553a0bcd5f1dd0f205058bc59b05423cc59b4f66c397792e30da7b50615a |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-4_comp_light.exr.meta | 6aee8f4fba73d56edb6bb672e354a90fb19288e229c2385f9026c2ba9d5e7f17 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-4_comp_shadowmask.png.meta | f7874a19049c0ce3a53589790359316c85e10adaa4e5b534343ded0c5f203039 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-50_comp_dir.png.meta | 979d253972c73ecc2d3d80f8c6d4a1fb5286228a4af9835764e61c3bb6d1e560 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-50_comp_light.exr.meta | 6bd0b0691917e844963881efea2a3f7310ccd7acb7e84736100aaadcb034c71a |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-50_comp_shadowmask.png.meta | 785d47a9b4dd43eafbe9b468c24ccfbdfb77523e83d7ab00ebf562843e8cf9e2 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-51_comp_dir.png.meta | 697013aaa4a650b218c4c3087718a809a6e4ad6917ad8ad2e6a75042671869c7 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-51_comp_light.exr.meta | 1bffa4c1adcf2905e924423a8a3891bbb694b0beb430f17434ffa11b083c438f |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-51_comp_shadowmask.png.meta | f8e47e62f8291c3bd8befba36cd3a8873455fff16f28993b34dd4c6206b99f1c |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-52_comp_dir.png.meta | 664d907570f6bd9fdc1a3ff9d669113a6c33d6b1a25f330613a141383f02257a |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-52_comp_light.exr.meta | 146372647b5b22a39a6cfa0df55aeb679c8d9aab90ba5ef7d6d4c5684fb46fa8 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-52_comp_shadowmask.png.meta | 233e7474b438c66174790d5c9769ac72c68a8f8f4e56343f2444468002773e0b |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-53_comp_dir.png.meta | cac28f2482717d7ea454a0ab1dd4e2434e062fda6f13fa908bacb2147b486b99 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-53_comp_light.exr.meta | ee52423f3bf108cd77997dc772bab760ed42f052287b70616a80583b2f09e66b |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-53_comp_shadowmask.png.meta | 5a0024124f8ce549a814d3170b3833f0cd33476dfb4f2b8bc02b77a410027d1e |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-54_comp_dir.png.meta | 62888c2cd41c88abe140953bb3216ad3777aa5a069da99419a38d155ceb56c53 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-54_comp_light.exr.meta | f475acae3b3365a50f8bc27867c4e29388a95ec61542d4e93ad09c4a99f311da |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-54_comp_shadowmask.png.meta | 8af399ca264803d7f9f7ae0b2dde54fdaafaa49c4800f9d5cb0ad1ccbf1aeabc |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-55_comp_dir.png.meta | 9f723857c448b3f1220b5a9d57145e36f2c58c7f53aa3fbf0e104dd3edefe3d3 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-55_comp_light.exr.meta | 0d6232d7fcc0f7f6efbdbd67c973ad428d22c1a46ec532e5686285a040b56c25 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-55_comp_shadowmask.png.meta | ec612b8f17ac2967b86bee36af201fcb32076dad03a258a8f57e1b55af950f8e |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-56_comp_dir.png.meta | 498a1149f1e88db21065f6386c363bbd07f9b0215a4ffb58631563937fe35408 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-56_comp_light.exr.meta | 4f29fd1a600168612827e79b6c1fd360512acfb7cf63dd80010dbe1219355905 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-56_comp_shadowmask.png.meta | f74c9b6018bafa83490d6712f69d7fdfd596c438a2af1a85f64e41f33075655d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-57_comp_dir.png.meta | 5646bf9f1684511de9eab4b5a3efa00bef2a9f5b5a5b1a1a30f2c4db5766502f |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-57_comp_light.exr.meta | 6f6aadb751a0e4999c1c0024128f7e10da4e90c6831a6550309bdd96112f0989 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-57_comp_shadowmask.png.meta | 5285000a4b0d713836c201fe08abbdd720d30f918490abe39050646c5966f9da |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-58_comp_dir.png.meta | bc85d25dc3991899e204fab56e762e94c3597d936189dbab2d026720e156d1d9 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-58_comp_light.exr.meta | 20dd43dc16ae9fcdbcf8b8a731a3176a113f24c880fe24f849b189b37683a4de |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-58_comp_shadowmask.png.meta | 76c20331601869d7c35e7ffc1de54885f4ecbc9571c58b6cbf4c37cfc52a9627 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-5_comp_dir.png.meta | c2743c1fd4f439ffb007d8a2cdea60b5ec840f8fbd713e6dc139cf3fc5544e2d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-5_comp_light.exr.meta | 67257e7b7624cf584c8d39bf078e4b2c2192a8e31d9fd1d4d00bb03b3251f3ca |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-5_comp_shadowmask.png.meta | cb2eee0d501867e552eb866e12d1b1291138932b301584d014ea0a3e630662bc |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-6_comp_dir.png.meta | 204366df3ca067433229a839d162a8442de03506cfc8034edaca443fa2230ba4 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-6_comp_light.exr.meta | 2cbf3d73dc9d3db97b38de53a891477ed8350336e0899270191d8854bbb120ba |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-6_comp_shadowmask.png.meta | 7d661b3c2254207d39ae30bc77d47106750a420e6f73c678c0c249bd0f4faa70 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-7_comp_dir.png.meta | 3040fff72a23cb810c74456b1c5099958ccc55b0a9b11e57f17719a1fdebd467 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-7_comp_light.exr.meta | 8136b3a9cc0a22614603c5b3d478cda9562f1aa56deaa9229f51be7f2bf4b164 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-7_comp_shadowmask.png.meta | 143a9e67d501cef94b87c934c6e5362e3440e4e0ba4c3afc45204ce0e7f79d8d |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-8_comp_dir.png.meta | 0b8537d90885110a00ba4479dbf8f169d9a473716405f9aaea9bb0072c4c14df |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-8_comp_light.exr.meta | e1dae70c9ee6638bb683a5c977dd22cb153a63740b71fbb940803e8bcd769786 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-8_comp_shadowmask.png.meta | c6867a4b3841a1ea935ed51ed1a6fac9b1a999550053c84fc0b657d88295d8d1 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-9_comp_dir.png.meta | 2bc949c88fd856ede0cac4bc046c5c2b5abddd185f466ea325a352b6540a25f2 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-9_comp_light.exr.meta | 97e379469c159457890ed0998970e91e16ee85155989312c4e84a7f43288fe16 |
| Assets/Holotna/Mountain/Scenes/Demo/Lightmap-9_comp_shadowmask.png.meta | 0489af12802f3346ee97e213fddfd174e451a260c14c3ef6d050bab18720c479 |
| Assets/Holotna/Mountain/Scenes/Demo/ReflectionProbe-0.exr.meta | 001e5e6cfef2887500c7be6807d9804d0da3d51764cc52c52ff781361a43485f |
| Assets/Holotna/Mountain/Settings.meta | d85d32598f86e7407ec57eb3d34d00d5ed5ca6961497b95dae3c21dea13a3427 |
| Assets/Holotna/Mountain/Settings/Effects.asset.meta | db55322ef1a509e03eba2593cfd5c134e4321cea47e8166c16f03239a7cf71e5 |
| Assets/Holotna/Mountain/Settings/Lighting.lighting.meta | b4d102e9c360aff4b7767db54182392ad5bb39c0df4503c32a90aa59cd3b5fc6 |
| Assets/Holotna/Mountain/Settings/Pipeline.asset.meta | 65a5fdd45d940ecd3d0172f32b95b05c3ced7ca841203186017ac937985f4d40 |
| Assets/Holotna/Mountain/Settings/Quality.asset.meta | 0b8fb10d2c50c1c4db67475e929883be08b3211fe5aa027e328a9fb52e108de0 |
| Assets/Holotna/Mountain/Settings/Rendering.asset.meta | 56f463b2c9a49b219ccc61035a6b266281f49bfe9c97035026226d8e42bf2f2a |
| Assets/Holotna/Mountain/Shaders.meta | 2a0c1da593d9f842e7fb4d360ee295acb262cfcd284fd9ec5e545df0c7e1b44b |
| Assets/Holotna/Mountain/Shaders/DetailWorldSpace.shadergraph.meta | 031118c5f96615f355fb7c5ebc1c0ec09e3cb30856f5483f0dd00fd86913577c |
| Assets/Holotna/Mountain/Shaders/PropMultiColor.shadergraph.meta | 43b335177a5f4ee439eaee66f5710e14fcccae9654ebd09e5d476e9139f37525 |
| Assets/Holotna/Mountain/Shaders/VegetationMultiColor.shadergraph.meta | 87245175de208433771a049a50c548f87d636e846ba98c69cd269243e0f22b8e |
| Assets/Holotna/Mountain/Shaders/VegetationSingleColor.shadergraph.meta | 575a5d5972348f056244977acfdea26957c8b17837bf9e5ca62bed444bccba83 |
| Assets/Holotna/Mountain/Textures.meta | 8a7d8fc795aa599ce8daa9e2bc4af00f02e6cc63611fdafbe8f1b8f29985475f |
| Assets/Holotna/Mountain/Textures/Bridge01_ALB.png.meta | b592e600eadaa10c25d0d350c9864771f4b3aa6aa17edc0a33aefdabd3ffa0f0 |
| Assets/Holotna/Mountain/Textures/Bridge01_NRM.png.meta | a925ca858780872b34df259abbee94d2b6d0180c69459d86c90ee9fae09024ee |
| Assets/Holotna/Mountain/Textures/Bridge01_SPE.png.meta | 1b322535db345dd2ce0c1b7d4e8bd5973a5613aa3ce48a236bd6009600680431 |
| Assets/Holotna/Mountain/Textures/Bush01_ALB.png.meta | 898ee5e2d2f7be29efa247a1c59d6ff4966d2876374ba1b29d06bc0e9b722d87 |
| Assets/Holotna/Mountain/Textures/Bush01_SPE.png.meta | 94baa897a0fb4092ded3e0105c542774237bdfddbf11947da9b0e4094a5fe9e1 |
| Assets/Holotna/Mountain/Textures/Colored.meta | 6831e84c140f5116e2c44f3b5a9f4d114e85a58efb44e9f1b3295df0188df560 |
| Assets/Holotna/Mountain/Textures/Colored/Bridge01_ALB.png.meta | db76b73796350a4c3f20ea34c6c32eecd4141d454f0c45ec2e5b78869bce2a3f |
| Assets/Holotna/Mountain/Textures/Colored/Bush01_ALB.png.meta | f62bd225e32e63d5df81f52b0d49b708d05c2b51bb305b79c3e4e955657fd00c |
| Assets/Holotna/Mountain/Textures/Colored/Flower01_ALB.png.meta | df7f9eaaeebf6040007d28080f9bbba6b58155cb5d320f09a03e1f834ba2e4ba |
| Assets/Holotna/Mountain/Textures/Colored/Grass01_ALB.png.meta | 7e2eece66164203fb1cb31f42b42f844ea7bbd9b34809c1864e564029ea9d8f1 |
| Assets/Holotna/Mountain/Textures/Colored/Leaf01_ALB.png.meta | 1bc4248114e65f71994dc617c0949d4a04a219217f397c455e1fb3f586967006 |
| Assets/Holotna/Mountain/Textures/Colored/Rock01_ALB.png.meta | 2769b113361fd24403d811a85912cf060358922c032b91bddf4ae15f6921f2cc |
| Assets/Holotna/Mountain/Textures/Colored/Trunk01_ALB.png.meta | 17f36dcca48620a8d2951b09aeb990b3d49a30d08bfe58de1d08594d1b898d26 |
| Assets/Holotna/Mountain/Textures/Flower01_ALB.png.meta | 673d5e7b71eff330f4d9759d025d952f382eae1aa6f9deab412ea015f14fb88f |
| Assets/Holotna/Mountain/Textures/Flower01_SPE.png.meta | c835e2d736a64983bdee5204b1833f5cc888dea2fae140dea5b72db8f31e3f35 |
| Assets/Holotna/Mountain/Textures/Grass01_ALB.png.meta | 7e7a7675cb8d29f06d946e2c980cf0dec671027e18174ba49bce17f4e155b503 |
| Assets/Holotna/Mountain/Textures/Grass01_SPE.png.meta | 77f21a11571536f06d319401de74fa0a3b08ec769cbbf3519ef0232eb54aad62 |
| Assets/Holotna/Mountain/Textures/Leaf01_ALB.png.meta | f519f86979fbcdf3ff64ed7b19eb74a248c00ce6648611a1f46f2d6667770a15 |
| Assets/Holotna/Mountain/Textures/Leaf01_NRM.png.meta | 7868163b7f04873daa483f365dd63f566140c790528c154f9c48eec696d5369e |
| Assets/Holotna/Mountain/Textures/Leaf01_SPE.png.meta | 638fc146a5201281f6c4b9a192bf04f4716a8a1b324b2c0b15bc00806c3ca97a |
| Assets/Holotna/Mountain/Textures/Rock01_ALB.png.meta | 6f02b94dba44e27d3f68150afc3d63a453c08a1d8be587600c82698124c9d391 |
| Assets/Holotna/Mountain/Textures/Rock01_NRM.png.meta | 78c6a1c4fe7eaa65b4db8dab65ad5418df3c3ecdb7ae270295ef1a402f326461 |
| Assets/Holotna/Mountain/Textures/Rock01_SPE.png.meta | 89d347d953726e3299eae7c68311dd3712ad127b3a5ee096211129627381cc12 |
| Assets/Holotna/Mountain/Textures/Trunk01_ALB.png.meta | 9c4277e0704bdb0e40456d3f3d9f07bcb6f3481267f5bc8a8a03156ec42b2633 |
| Assets/Holotna/Mountain/Textures/Trunk01_NRM.png.meta | d6979bdcce14431a0423d62597bf33acac1a1738b6cf89fe417c9968ec6cf55d |
| Assets/Holotna/Mountain/Textures/Trunk01_SPE.png.meta | 8e978d4154297eeee8fb80796c65a441eae8aad89181b26357d0152e50df497b |
| Assets/Holotna/Mountain/_HDRP.meta | a4e66b93ddcff930486a98880c0d6a4285a1d778d5aac9ac60c694087e5eceed |
| Assets/Holotna/Mountain/_URP.meta | ba499088d069e375982f6b490e3b5ed4e22757ef034fd66315c4fc09a0f7b43a |
| Assets/Import.meta | 525050f03eb5b042c856ef713437d60730a7bc1d33530fe558c58929e529db27 |
| Assets/Import/1.fbx.meta | a9dfae32e0a6475f45173c80e740cd5c96f13896513494b5acba6ac71d4591fa |
| Assets/Import/2.fbx.meta | 50a7fd8f7bff92f2ded5d599eb73f77296e11cd0e11317d4d8f745bc84813b86 |
| Assets/Import/Asset.meta | faf6b49024785e3d7888da476891be54a432b1c9620659d0dbeae784a63c872f |
| Assets/Import/Asset/Font.meta | 6cd579ff0b2cd9c43f9248c223634a4cf05f1d19376171da8c36499ac02f0c9c |
| Assets/Import/Asset/Font/Babylonica.meta | ae368285756a30aeb92458cd895a189895feea3d64d4b238212ef78cd57fcc57 |
| Assets/Import/Asset/Font/Babylonica/Babylonica-Regular.ttf.meta | a7204fd6dec611c485f5b3d110a1619a37b4922ca0fec206cb210343f9ef4a1a |
| Assets/Import/Asset/Font/Babylonica/OFL.txt.meta | 8cc606f0fd87ab3cdc5da640115dcca7eb23553289e39d9cfb250caf2eff9e56 |
| Assets/Import/Asset/Font/Cormorant.meta | b2997a7377fd61975e012fb3e3861821763754eebc01dd5ff429d0b475368e85 |
| Assets/Import/Asset/Font/Cormorant/Cormorant-Bold.otf.meta | 4324369cb28dbc7b03c21fb2eb7bac816fdd3e99c88f1a977ad7a6b5b5a1b817 |
| Assets/Import/Asset/Font/Cormorant/OFL.txt.meta | 5157bd20e9d6ac427a69a06121e42d37de2e41efb904b5e3101093f500726332 |
| Assets/Import/Asset/Font/KD-Hidayatullah-v2.ttf.meta | f746afb6b5eb0d9a83cab45fa2569e6efa3977c14e520ae46c1186ce9daa5d34 |
| Assets/Import/Asset/Font/Source Sans Pro.meta | 556e6a24b82a7868d8d002c24cd2c9840a242c0dd55a3ed82fe9f4211cc4f83d |
| Assets/Import/Asset/Font/Source Sans Pro/OFL.txt.meta | 49204d5777907df3663149aa480ff40f864e284ed4b8ef1b479808dd52437d88 |
| Assets/Import/Asset/Font/Source Sans Pro/SourceSansPro-Semibold.ttf.meta | 67a8ccdcc8aefe29ff8f4fe1606021ccfa018660b09703571f0a4f3393265f2b |
| Assets/Import/Asset/GameBroken.png.meta | 0c11193b258780c86b31bc44706a2fe847875f58980b38f8016a69ace4d69c75 |
| Assets/Import/Asset/Khung giao diện rồng vàng huyền bí.png.meta | 68e3511fd8d7c8421fe99946777d6accb2ea16acc3b91f1502ba7e03deca2c30 |
| Assets/Import/Asset/Skills.meta | 6aa5d4fa279d086c3f75c5b176e386c237a62a88e30c208f9a66b42cd572d403 |
| Assets/Import/Asset/Skills/ChuongAn.png.meta | 864e508f33f3e2ed99abd64f5ef2c80f0d3a2a91861e89cd3243ce9861a0bc6e |
| Assets/Import/Asset/Skills/PhapTran.png.meta | 7609c3a122fee818c35af38394a02399a725be0d878342ae4a5d3f2dd27d23ca |
| Assets/Import/Asset/Skills/SamSet.png.meta | 082a810892b3862e311df646d5c389c2b0146cbccab17189061954114cadf73c |
| Assets/Import/Asset/Skills/VanKiemQuyTong.png.meta | 57dc958f71a2c7f465eed302df498e4eec01dbca204b3263169533929c58e863 |
| Assets/Import/Asset/UI.meta | 328e018e2106fc445f8c348d970e1c03af24ef25a3ad55b58c24718f8bc9a45b |
| Assets/Import/Asset/UI/Bộ giao diện game tiên hiệp xianxia.png.meta | da5cee0df91f34e7ed0959ed60e11704f81e517ae9682480d8ace0c22dc0126a |
| Assets/Import/Asset/UI/button-removebg-preview.png.meta | b5c1ce7b144beafb86503a3b6b7d2794c555f9332f81fb944195f2f9ef45e50d |
| Assets/Import/Asset/kiem.png.meta | 5406eb9625d1233030028b214ac26cd719399298e06f80da2123cfc82d3d491d |
| Assets/Import/Asset/loading.png.meta | 34e1d4482096dd1384834cdfc63443045793e84cabb3440f150e711bb946a11d |
| Assets/Import/Boss.meta | 8112bd3ceca02cb0ceec866d8ad852629174a89742e6a18a99da7c3523d79f59 |
| Assets/Import/Boss/Idle.anim.meta | ea50e5797d700aac684eb32723edf481046b5b6a8a0c27b603a773fa59b7044a |
| Assets/Import/Boss/Skeletonzombie T Avelange@Idle.fbx.meta | e7811b903996c0dcb161e12a34897a011c53b9bae6854cfad8bba18bf1d2b126 |
| Assets/Import/Boss/Skeletonzombie T Avelange@Zombie Attack.fbx.meta | 36378ff35ab79a04d767082a94bc2d58a26b1730b71e49dc1a1c2fa89f4ba388 |
| Assets/Import/Boss/Skeletonzombie T Avelange@Zombie Death.fbx.meta | d38ee632f23e8b1990f27dbe5f4c5720c7eb407f6dea720cf22b8154d17fb2d4 |
| Assets/Import/Boss/Skeletonzombie T Avelange@Zombie Walk.fbx.meta | f03dd12a221430bfd65892ab62d530751932767537fc799d4c8695a60ec37f04 |
| Assets/Import/Boss/Zombie Attack.anim.meta | 9b7ad9e55b080a4d0e8e27a5718077056c0256042108b7d6dd79ac363848ce4c |
| Assets/Import/Boss/Zombie Death.anim.meta | 5e17654d523c087775f6ed869ca433ad4f4baac69bcbbe9f47d14a92bb48457f |
| Assets/Import/Boss/Zombie Walk.anim.meta | 4a0e1cf8ecc64574c4e333a96a2a49e5de061fa50722525f19a18bc8801ecb78 |
| Assets/Import/Boss/boss.controller.meta | fbfa17778449401e67073d105d15ef1d92885822e64cc972ec1f3dd46488ef9b |
| Assets/Import/Boss/skeletonZombie_Material.mat.meta | d556314c9c5f1f8c30a8eede81ae741931dd9f11844ba777761a5868b762871c |
| Assets/Import/Boss/skeletonZombie_Material1.mat.meta | a7cba8f0a6e2e848921ad648b7b8c7823b41934691a7f7f3d485ba9c0fe34039 |
| Assets/Import/Boss/skeletonZombie_body_diffuse.png.meta | 390620b90c2f2b8acc56262c8cc7fc9818003d0c178ea9cb342dc12cd054281d |
| Assets/Import/Boss/skeletonZombie_diffuse.png.meta | ede8a05dc0930d653e13bebfc5b7b4df434d535a157c919bab2dab4b32e64e76 |
| Assets/Import/Boss/skeletonZombie_glow.png.meta | bc7f3b93bf1a4eb96da88611c45a85d9a259c89e751e087bd1a8dd52579ae0ef |
| Assets/Import/Boss/skeletonZombie_normal.png.meta | b89800d4283be8b9ca09dd1f15d45d16decd0215cb1ad8b40902df1de62a114e |
| Assets/Import/Boss/skeletonZombie_specular.png.meta | 0aafb00732f21dca875887bb60e2eb2dec0a0d9162608654656081f10c9b9083 |
| Assets/Import/Cõi Tiên Đảo Trên Biển Mây.png.meta | 7e365014621e8e129dcc9d861a6d7855782488a177f7066ae8661a57767f98b1 |
| Assets/Import/Dead.anim.meta | 2fd95ff065d1895775e570893e668c69a2a5c2cb9f94422df30cd2d8be3989eb |
| Assets/Import/Enemy.meta | f2a8b07344cc5f09716748fbfe5b112ceb67ecd4f0e04fd801a63d20d3cb7446 |
| Assets/Import/Enemy/Enemy2.controller.meta | 643d1d92bcc8024e4502021996b6c29e2fa539a5ec5c0ab585f6da8a1f8cfdab |
| Assets/Import/Enemy/Idle.anim.meta | d54acd2a1b514cf271191a31de7e62898bfaae700eebe3eb8cda62243cd71f80 |
| Assets/Import/Enemy/MAW_MAT.mat.meta | 7d606ac44c8d4452c8e16846ec6246d5a01a677edfd5962253eb586a462f1799 |
| Assets/Import/Enemy/MAW_diffuse.png.meta | 79fa782097932a37689c0fe7b7cb3793bc64d7ac1bedc558f6dc8b8b52728455 |
| Assets/Import/Enemy/MAW_glow.png.meta | af6c565480c3491252c1f16fb211f5800683f39faea9c4defc0131f7cd1d4aa8 |
| Assets/Import/Enemy/MAW_normal.png.meta | 823a6a30d987ba9bef5e8048f4f8c078d9182c54f028ef66a51782d605ede74e |
| Assets/Import/Enemy/Maw J Laygo@Idle.fbx.meta | 3a76315a9728c788384c76fbf38fd85e2421a7c6d04a0c56f225003f25cd8655 |
| Assets/Import/Enemy/Maw J Laygo@Standing React Death Backward.fbx.meta | 505ada55e62c3c339c31e213a94ac82d1e7d8217db4029a9a296ce79feddbaac |
| Assets/Import/Enemy/Maw J Laygo@Zombie Attack.fbx.meta | 8d49d5ca551021012194d1347fb8fc165762592e4a511dfa3373bdd81c8c3300 |
| Assets/Import/Enemy/Maw J Laygo@Zombie Run.fbx.meta | 8ab1884cdb404a96dd1d57410f441363b77c5da91df36f784aad51eac337b12e |
| Assets/Import/Enemy/Standing React Death Backward.anim.meta | beeb61bcb5b832f61333a045919f1eddc023f3d0356e9d184fea17fde911b1d9 |
| Assets/Import/Enemy/Zombie Attack.anim.meta | 075f829e2f28ac267d79734fea6de559f2549a021035b1baab92a163570bfa05 |
| Assets/Import/Enemy/Zombie Run.anim.meta | 19fd41f251e62349fd53b87bdde146ce0c6510ce6ec3f870720653aa04db1e02 |
| Assets/Import/Enemy/maria_specular.png.meta | 9a7d6642865d6ba8a4682d17cd011cb4541fc39799bd83f8b276b9465330f989 |
| Assets/Import/Enemy1.meta | b4a10cf4a4ae987c4cf2f7c0b07afa1e2afc183338e14f300ea599da764e856b |
| Assets/Import/Enemy1/Idle.anim.meta | b93fa773fb10f7be9d8adb328b54daf10470e1d2d665cdeee36c15bbd02e1101 |
| Assets/Import/Enemy1/Warrok W Kurniawan@Idle.fbx.meta | 7b4b431545bcc6fced8344c14558aa0bea577848033ec9091199e80a4dab1fe2 |
| Assets/Import/Enemy1/Warrok W Kurniawan@Zombie Attack.fbx.meta | 45bdda7dee2ff70de5e0da4e8674ce558f125a78acc3b7b7c7f8b23dfe1dc728 |
| Assets/Import/Enemy1/Warrok W Kurniawan@Zombie Death.fbx.meta | a1b62e1d2477a6c569d7887a2c6a27358a6693bba9d227ec94f6ad00ce381ab0 |
| Assets/Import/Enemy1/Warrok W Kurniawan@Zombie Run.fbx.meta | 95b61087737d41107f5e1b0105a8823d9283a092730514c8e32f035eb6f78b83 |
| Assets/Import/Enemy1/Zombie Attack.anim.meta | ad897367d21f0a8179d52680f3af1f3ff5759fcafd684f05f3b00097db837fa8 |
| Assets/Import/Enemy1/Zombie Death.anim.meta | 4241159a22f5b525002fc70be66cb6960d0db8a0ad77c9118151035dbd49762e |
| Assets/Import/Enemy1/Zombie Run.anim.meta | ef532e6961ec4ebfdc896fb84cd8c0428a8e5a698e2efea980a6cb0de2dc9bd5 |
| Assets/Import/Enemy1/bear_diffuse.png.meta | 22300ba2403b14d7e4c51c4102697fe2416144c59902ebbffe45ec2e90fe51a9 |
| Assets/Import/Enemy1/bear_normal.png.meta | 1f2b142e851c2d7e1a3d0ad9be0f491f03c51216d88cabbdb939e209201958d4 |
| Assets/Import/Enemy1/bear_specular.png.meta | e6632f18da81130af0c0bb9fd73e1a985a01616b4c6b4edb1cd67645eb3c47d5 |
| Assets/Import/Enemy1/enemy1.controller.meta | 88409efcb1dada209c65e68d671e89c4bfd0d26e5d07eaa3609e10298c525d4f |
| Assets/Import/Enemy1/warrok_MAT.mat.meta | 1dfb6c0eb6083b515a4adb7198411c37919b21ecf374e217c582752090084a1d |
| Assets/Import/Idle1.anim.meta | 8baa017e107d6a4d7f5e49f988e114c910286b6e3d88a67e3f6f76066c7f43d3 |
| Assets/Import/Image_0.jpg.meta | 094fd1151602610d35f137f1e35306d3f77a595c904b6e845042172f404a90f4 |
| Assets/Import/Image_2.jpg.meta | 85e8287ca46b1859bbb69bd0811b7887ec384383022c687fa2b145c5f8002b3d |
| Assets/Import/Material.002.mat.meta | e8296bb18fcf9df6f75bb1ba99af23bed18bbf98cbdc2c59bb882e674e479d62 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped.meta | 52feee25549c54e5e8304b918399c1dd201795369630d19e9e4205ecbb099d35 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Image_0.jpg.meta | 07b167e0d324ec4891bc81834577caee3865aa06390c44a2b0cdd9747237d78c |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Image_2.jpg.meta | d0c7a1dfbc3c91147c78ac648a079bd3fb3b3e5c69c427da3cfc8d74f43b2dc4 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Material.001.mat.meta | 55d43923f133ec76e2692c03e96394c06c480cd5802a0839d6ffe4cef419e9da |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Material.mat.meta | f9a75daab6b20aa82a878991d5d3e1e1754abb812f14141ca0c11fe56581dd75 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Meshy_AI_Chibi_Crane_Cultivato_biped_Animation_Casual_Walk_withSkin.fbx.meta | e595881529a0b783f0f7091802c824d09db3bc4eb438c1f22c83c2e546718eee |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Meshy_AI_Chibi_Crane_Cultivato_biped_texture_0.png.meta | 2dfc0833f95b09325d2817313c3bd975f148e6e299530a2f30f5e2cd447f6fd2 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Meshy_AI_Chibi_Crane_Cultivato_biped_texture_0_metallic.png.meta | b1112ee760a42625f25dcdfa04ae855106686829b97aa808574a3e706899856e |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Meshy_AI_Chibi_Crane_Cultivato_biped_texture_0_normal.png.meta | a98383b00a4cd13d1f5a4fcebab2e7d20c46fd9ff8a1258902644e7c35522944 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Meshy_AI_Chibi_Crane_Cultivato_biped_texture_0_roughness.png.meta | 76cd812472a45c2ca598831a65d948ec8bf8de4778b42c1107d8badecc2eb16b |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/New Animation.anim.meta | 5237e60c3f68440dcbf3007c22d3da42258790e4cf2cc5ab309cd65d43275cbf |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Player.controller.meta | 03435eeede10f6274e36232f208e977cd0e09c0b9e1118685131a700d3958ae0 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/texture_0.png.meta | 29aad7ee07afc2d4d2b4e8b6a023f6783a865563c8d6000d482212c65b55cb54 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/texture_0_metallic.png.meta | 7797e0ce685f4be1846a2a67bca6e469651463342f7e94cfb14d9800a3e9860c |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/texture_0_normal.png.meta | 1560c64057cbba76e957d4e900b14491f6c32964a198e1b0abc7799e4d93d6a3 |
| Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/texture_0_roughness.png.meta | 99a374acdd63af3f6d38f1067a80c88d4215d5baa8db8c62c60b1443c4f4f33e |
| Assets/Import/enemy 3.meta | 461d7257cc823109e8ec65e9ef22682b1e2bf2f4562c3c3621ba4dc706698957 |
| Assets/Import/enemy 3/base.fbx.meta | a3e68af4e969ed607a55f783a99473e667fd2034ce5bd083387d1462e2a04123 |
| Assets/Import/enemy 3/material.meta | fae0087bec01404fcc77750abcc2fb9126e6c12b53c62c214f83caaef69dc847 |
| Assets/Import/enemy 3/material/base.fbx.meta | f09ed8cb8dc7ddd81b15b30f92958259275b6c53918c8b9f47fa0bf0d51d937a |
| Assets/Import/enemy 3/material/base_basic_pbr.fbx.meta | 1734c97edc828c40a2710a77aacc864d888fd4e59a0cb097fa4d65efb8467514 |
| Assets/Import/enemy 3/material/base_basic_shaded.fbx.meta | c87befda0e0712e2d7659948c47d37c733cb03d2854cadbbf4fbd62713dd5cea |
| Assets/Import/enemy 3/material/shaded.png.meta | 3a979a5e4b50a67e7f0a018992670af79eab480b8b78bd41dee1218b49f83c39 |
| Assets/Import/enemy 3/material/texture_diffuse.png.meta | f6240ce81a3be5235256efe78c476690a2683248b0abaa45df10690cfb24518b |
| Assets/Import/enemy 3/material/texture_metallic.png.meta | 5a841e377423d3315ec6f6d5fc81ed0a858f0c0e02eea934c392effbdbd99636 |
| Assets/Import/enemy 3/material/texture_normal.png.meta | 09deee5388faa04f2f14ec8da869af5e37715ac9ca2c5d94795a7f7111ded5b1 |
| Assets/Import/enemy 3/material/texture_pbr.png.meta | 14ee6d316b79652d5db69695699045362e924236e93b116a6cb7cbf0ca98566f |
| Assets/Import/enemy 3/material/texture_roughness.png.meta | c9c06419023d1b251743436f54543b5b89e470e19e5a17b757b6000ea59456d6 |
| Assets/Import/enemy 3/model.mat.meta | 7f9c4bec78b762bdfe2149e444152beed583bc3e774bb21e402fdc33dd75a926 |
| Assets/Import/idle.anim.meta | 1ae4476b29327482063cab402376b970f761f47f2332723248b69a53f4a70950 |
| Assets/Import/idle.fbx.meta | 64f232e8b6373bf682f99184c383bdcea0829fd891b4b1b074d839663f4152cc |
| Assets/Import/player.controller.meta | 1cb2e1950fa38e6c657d930d299fefa81f36c724ea20758e07732959a40db658 |
| Assets/Import/revive.anim.meta | 429c94065c58c7d6558e470251970195aea60f1f55ce8227ecbda1644002774e |
| Assets/Import/revive2.fbx.meta | 1c1a36efafd85bd67c990a89b52bb717062dfbc444bfe96d56b7fb853801baac |
| Assets/Import/texture_0_metallic.png.meta | c4499c8c232ebf45397585deac7d1386c49d07cdb4374de52cc4f53a7ce8612c |
| Assets/Import/texture_0_roughness.png.meta | 6cd89bb9e2415dc16e05741f8ae189a52b4a20cc3064b4ed28c558968f81adc0 |
| Assets/Import/Ảnh chụp màn hình 2026-10-10 173048.png.meta | c1f23189ee930554b2db45bf9a2169404d3fc1a7a7f11dd7331f23bbebf6e973 |
| Assets/InputSystem_Actions.inputactions.meta | 3b4e424a80f5b4fa76b6abd774884a24285eaa61f22ff9bb20e5ad505fd8b569 |
| Assets/Materials.meta | 31dbcc83b75bf0019ccded632c8abb2a6427153c6719ee7e3b02b65fbf53d5c3 |
| Assets/Materials/Mat_MagicArray.mat.meta | 82d7b0132a54f54af03d4d81bd3db747036b22bfa03f4c734590ea1dacb10e8b |
| Assets/Materials/VFX.meta | b700a9c21e67cfe8b89e7d8fafa8f57d1b67697d4125c6dd2da8ffa1f89817e9 |
| Assets/Materials/VFX/Mat_BloodSplat_URP.mat.meta | ef76edb520fc677d028ad3343b0ee9ff78cfcd2731845e49b6b7ce160c3a8037 |
| Assets/Materials/VFX/Mat_BloodSplatter_URP.mat.meta | 712c82a3fafe4756675c176fa80200adace42b36911b7e9ce09e5972873bc641 |
| Assets/Materials/VFX/Mat_GoldenPalm_URP.mat.meta | 03d60b96066b89ed8e89f8340ce73f2be5774f39af04833fe119874e33cb5a97 |
| Assets/Materials/VFX/Mat_GroundTelegraph_URP.mat.meta | 80abf1991826da8574bb0ef10be81061a62cca78b67e8f9892b2fcc32c8e0390 |
| Assets/Materials/VFX/Mat_LightningCore_URP.mat.meta | 8f176eb929cbfcbdcbc1e9bf75ff6337c4df5af90526d4c5f06c9ffbd48ce9a2 |
| Assets/Materials/VFX/Mat_LightningGlow_URP.mat.meta | ecefc0b402c88e63201b5fcd7ecc78831867f26d9ceeb7246f97ffed255ee008 |
| Assets/Materials/VFX/Mat_LightningSparks_URP.mat.meta | 73798d8c70cea0e51b5984c5c293a2168859c8bfd92569a1ccae4270dc3b039a |
| Assets/Materials/VFX/Mat_ShockwaveRing_URP.mat.meta | e438e17e5b283749b981dcfaa348f01aa2c59eab5d174fd3b6ce4d9f7cd7ceac |
| Assets/Materials/VFX/Mat_YinBlack_URP.mat.meta | 5a1815b930d63a48173452d9861f3a48ec2adc7b82bb73152605865e7c873fdb |
| Assets/Materials/VFX/Mat_YinYangTaiji_URP.mat.meta | 83f5ada6dc68ed25dee5f17e4f0dd864b85878fffadc2e95f1bce0b2ed9aad36 |
| Assets/Materials/VFX/Tex_VFX_BloodDrop.png.meta | ec62df54aed9e61a80b42eb40a6e5febf23890a1ab398171fc1fde278b890529 |
| Assets/Materials/VFX/Tex_VFX_BloodSplat.png.meta | 5d1b7362f8f0159c885946e63ad3b5e90ade2d62acf747d651c1399c413016f7 |
| Assets/Materials/VFX/Tex_VFX_HitSpark.png.meta | 702235d5f6628792e30645e361e6988bc84165934a0147dc1c92b73e880de054 |
| Assets/Materials/VFX/Tex_VFX_RuneTelegraph.png.meta | b099bf34817f26e8eda951b86939f87ea2f405144319e94bf1c363eb9173a18b |
| Assets/Materials/VFX/Tex_VFX_ShockwaveRing.png.meta | 8cf7272a0550d97b827710f965dc0dd0f309869fb9ce8619cc9da46c09bb75f6 |
| Assets/Materials/VFX/Tex_VFX_SoftGlow.png.meta | 676b9dca36e75eba9f45c08fd50e09a0bd739ac595b27bfa3ae0bcd8e3176432 |
| Assets/Materials/VFX/Tex_VFX_Spark.png.meta | 7744614988b3c28a3f41e247d2d80658a6d245444b7a6f9ee29dcd8b70e6076e |
| Assets/Materials/VFX/Tex_VFX_Taiji.png.meta | 76ef7e93c738b59802c9badde325507c0eced140cd2de3914e7eaf6ba720cca7 |
| Assets/Modules.meta | f7c7ee382635eedb13693bcf0d22ba09f92897fbcacee5bee02456bb78976c6c |
| Assets/Modules/PCContent.meta | 56929b92e5561f53cf5203688087d36e90dbd8b8db5d8c7e16fbaf4419ddacbb |
| Assets/Modules/PCContent/AGENTS.md.meta | 02e19fb63c006273078b2b87cac05d2ef5e74f392f03f1338ed66fd2f2a1c37a |
| Assets/Modules/PCContent/Content.meta | 28469fc61d6492787b920badec653785044b5f57448204acc63ed3499e91cb1b |
| Assets/Modules/PCContent/Content/Opening.meta | fb38f849e3f082a7b321e8139e7405cc86dd968bf8a8ad9fdc459cbb0adc47cb |
| Assets/Modules/PCContent/Content/Opening/Materials.meta | b23d6a601da3e43d45482fe20008d3c06e09e0f041516ce135bb0dfdf400a304 |
| Assets/Modules/PCContent/Content/Opening/Materials/OpeningGround.mat.meta | e12e9d66e96854a555c8776d792c46c38c0f0bcd7c747ef08f69f9821db11244 |
| Assets/Modules/PCContent/Content/Opening/Prefabs.meta | 9448ad6060b1cd99cf10a903d9e341e0c55a18f307fa677341e04fc75db413e3 |
| Assets/Modules/PCContent/Content/Opening/Prefabs/OpeningChunkTemplate.prefab | 21ecf3e893a394fe793fd04e80e84f1468c8068dac8284e023d20926e31991a8 |
| Assets/Modules/PCContent/Content/Opening/Prefabs/OpeningChunkTemplate.prefab.meta | 6fae98f682fb17bb907cb0aa47f4071bf674c31b6f7f02de19707999b78c40ff |
| Assets/Modules/PCContent/Docs.meta | bed319f1a3576df3025f26d35dca640bfd572e9d243f3fae5bcbad479ffb9ee7 |
| Assets/Modules/PCContent/Docs/ANTIGRAVITY_PROMPT.md.meta | e6c15ee81193ff649de9da0ea212e3ecdd1f15a7e29bcf2a17785e079a6fdb72 |
| Assets/Modules/PCContent/Docs/CONTENT_CONTRACT.md.meta | d7605687a189086498554c6223e86f65419387d6db4286fd7ddc5a76da6dbe7a |
| Assets/Modules/PCContent/Docs/HANDOFF_REPORT.md.meta | c7772d3b10791bea4836d4da44087eb71af808a926c0e1c4c0ffacd4df9d37fe |
| Assets/Modules/PCContent/Docs/QA_CHECKLIST.md.meta | f561c05bbe1150959f9b64c8fa6a5e88019d036f0794387ec521f54c5d47f76d |
| Assets/Modules/PCContent/Docs/TASKS.md.meta | 5e1ba65020a12ccfaf913bc996203cc8157a7667eba41e384c6c939b7279c536 |
| Assets/Modules/PCContent/Docs/UNITY_CLI_ANTIGRAVITY.md.meta | 9240ab3ad799e34f222215b76c8ccb3bcd777e55c9d0d71e0f6441c5822c8517 |
| Assets/Modules/PCContent/Editor.meta | 0c9ebb21359bf9f71605dd5bacb7ae2fc276b1cf4e518e00c005631b14750175 |
| Assets/Modules/PCContent/Editor/KhiVanChiTu.PCContent.Editor.asmdef | d025d3283a26e55964fd111f2a79b1a6856286a6fa5a3f7dfcafd04c5c74fb56 |
| Assets/Modules/PCContent/Editor/KhiVanChiTu.PCContent.Editor.asmdef.meta | 931158948b19d694949884155fbb804c8c2dbefc100a756b486bd44b94d7151b |
| Assets/Modules/PCContent/Editor/PCWorkspaceTools.cs | e68a084d84954aa0acfbb758fe1b6173ec04b65813ecd1cb561b8e8412f405f0 |
| Assets/Modules/PCContent/Editor/PCWorkspaceTools.cs.meta | ad319259860f14640bd637ba29bc38da2ae48151fe1aedfa2fd0558963c31660 |
| Assets/Modules/PCContent/README.md.meta | 48fd23e48434595ace5b5d2ab0c0c4b476cb31dcf28bc29a834f5d3befb7da06 |
| Assets/Modules/PCContent/Runtime.meta | d3cd0c3f9939357a9816acbba6c999fc59c7ff4c62e1c04aca03a355b1763dae |
| Assets/Modules/PCContent/Runtime/ContentChunk.cs | 878052e0e80a93a12b33ae5441ca8eea507388f6bb1978eeb3619e5dff0dbd2a |
| Assets/Modules/PCContent/Runtime/ContentChunk.cs.meta | 427f4847fa81070e1ec0c81355ff8bd7ca80c0573c27d895db031235287fcec4 |
| Assets/Modules/PCContent/Runtime/ContentPreviewLoop.cs | b047e30c3992cece2a5e44b705fc7ea26e722738ecaf0532fcf4e8abcf1cdaf8 |
| Assets/Modules/PCContent/Runtime/ContentPreviewLoop.cs.meta | 05fdd569c9d13f9c869eb3a939046bc474e33b478fd87522c4c07006e4bdf283 |
| Assets/Modules/PCContent/Runtime/KhiVanChiTu.Collaboration.Runtime.asmdef | 6c889ececcec1ea57ad31efe56ec19c89452e342eb143ce446e38fe253fd444e |
| Assets/Modules/PCContent/Runtime/KhiVanChiTu.Collaboration.Runtime.asmdef.meta | d90dce95a637ea2c8518b7c662fe4ea863197d26aa1827c031ae9e851af509ad |
| Assets/Modules/PCContent/Runtime/WorkspaceSaveProfile.cs | 23b671d4acf89bcebf5b391d3338dc42c004a7ded01a225e001114cca70fcd4e |
| Assets/Modules/PCContent/Runtime/WorkspaceSaveProfile.cs.meta | f2ab470de325f598003806985f2f1bcebf85cf3e9d873b9934dd5083aeda46f2 |
| Assets/Modules/PCContent/Scenes.meta | 141fc3f786e87880820a6591380a9785709fb593e9d7a6ac9b3217044a7463f6 |
| Assets/Modules/PCContent/Scenes/ContentSandbox.unity | 995a52095fabb0e57d99e2be4ce77ce298d2deef995dfab175b58fd2a712f5c0 |
| Assets/Modules/PCContent/Scenes/ContentSandbox.unity.meta | 969c167d79d2d45a8cc661912e5ef640898ef6f872b54421df6e301aa2ec13f8 |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity | b04e781db9ec0db429fe41291d1f8043ca184306d434ce1897afb949b967ab9e |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity.meta | 1fdae3421b3019c6e48a94edd3693f1e2e6cf2ff3270fc1c0de0d8526d2d5c39 |
| Assets/Modules/PCContent/Tests.meta | fb9ea4de873ac90b08580a85fad3e8a3c327f2ee47f907f897faf6593953674d |
| Assets/Modules/PCContent/Tests/Editor.meta | 97d44cbfacc2087d000f6b695a02c3a9985e6a1497b1c7bb0b61f0d456ee5476 |
| Assets/Modules/PCContent/Tests/Editor/KhiVanChiTu.PCContent.Tests.asmdef | b051cb1dc9b50f782713187cd239a50cae924e1f8923c83cf94bc8dce127467c |
| Assets/Modules/PCContent/Tests/Editor/KhiVanChiTu.PCContent.Tests.asmdef.meta | a5a57ff9f9bf9375f68c518a2f92d7371a2d8cdf470423b2fd8e35c557451c7d |
| Assets/Modules/PCContent/Tests/Editor/WorkspaceSaveProfileTests.cs | 70b950fb52af9093b4c8342f4fa45973d7c57e66024d839dc4e8b98662b9f0b8 |
| Assets/Modules/PCContent/Tests/Editor/WorkspaceSaveProfileTests.cs.meta | d17a8989f03c2306e0ade216377b4e9bf3b9fb471180df9eb70e1f8af1c8ff30 |
| Assets/New Terrain 1.asset.meta | 9de27072e3148c96b74d1af6b804f55aff56e4f26b527b7831305fab8e33add0 |
| Assets/New Terrain.asset.meta | 0ab197f427ddb91648b310956edb130283f04d8a9615c39f5eab8ea549290103 |
| Assets/Prefabs.meta | be9520af6bd92d710707bd414d6e1f5ad01f08eaeb22cb37aaedd9600f3c6da6 |
| Assets/Prefabs/Boss.prefab | ae421a472389ab46e2713373b164e0e329318567c4cad18d2ee09c9c4350bcd5 |
| Assets/Prefabs/Boss.prefab.meta | e29f110eff93adc6a0c3431df3f662fd4910edc184684c3ce2e180348717e9f7 |
| Assets/Prefabs/DamagePopup.prefab | 92bcdcb531c91523d5f6c4c310f70837fb3e3e1ecee36ad817a59fa559e42a4d |
| Assets/Prefabs/DamagePopup.prefab.meta | 13d4456e40a28da6616f91feb51e43304661fff658095b3073ae5981a8f55a34 |
| Assets/Prefabs/Enemy.prefab | 77e92db1b13fc3fb3aa2e56e84ba9e6e3992996fbcfc63908609fe6df9629831 |
| Assets/Prefabs/Enemy.prefab.meta | dd3f6e2338ab24863c36ed1112da560507f18932455eb4aadc05e2730b588b36 |
| Assets/Prefabs/Enemy1.prefab | b20ce17aff70cdda9c9850420e7fbf565fa0bb72fac301591c34ed6a5f9e2e21 |
| Assets/Prefabs/Enemy1.prefab.meta | deb7429a781d972a57763070c6f07d3fb76f393e0e2eaf131c73714c00c7081c |
| Assets/Prefabs/Enemy2.prefab | 56b2936fa659b1de2e9b7cd7ddedc6509330d87cfa0e2ad3ce1367795dc5e5a0 |
| Assets/Prefabs/Enemy2.prefab.meta | f34c762e7c955b5c31f016ab950ce334030269350e87e17aadf1a8cf7ea44d36 |
| Assets/Prefabs/FlyingSword.prefab | f7edb42a207ccef81c668a56302adc0fc882674795a452da9aad4a1db9da4fa1 |
| Assets/Prefabs/FlyingSword.prefab.meta | f7d4e85f25dbf28ae56d2c016b7f5e74df6acee0fe3c957b9bf5ce596602c5cd |
| Assets/Prefabs/LightningVFX.prefab | 83b00141bf7aec741d7bef08e94c1bca47b94396cc7275be51479608f58919cc |
| Assets/Prefabs/LightningVFX.prefab.meta | 523d52620c7431074529c05eb2bbc0d2da62087ce94be1bfddac6a2dd2be9668 |
| Assets/Prefabs/MergeItem.prefab | 5b7d5c10f9bb3ff3a39c7b923d0b9196dc7d5cd8dc5cec6ae33d32ff87a6e086 |
| Assets/Prefabs/MergeItem.prefab.meta | b640577572c3e80fea5cfbc0c3f87acba27e9c4c51e90a6e2d1f7a555e6dced0 |
| Assets/Prefabs/VFX.meta | 2dfe90ffea004a821e46b2dd0cb59e4a4a5a85329193965b4d6b197bfe6652c5 |
| Assets/Prefabs/VFX/TribulationLightning_V2.prefab | d76353ea983e1f5280b78d25892e2364b9b301e1457b395bf8d0f982fee6f012 |
| Assets/Prefabs/VFX/TribulationLightning_V2.prefab.meta | b57c558f2320cd60691a929df40b21c90a8832928ac8077ea7d5e64092946cd3 |
| Assets/Readme.asset.meta | 46c6b89ac1d47bd0c0d1a8b141ae3e32cd95d3e74c49381f0d9f942488c254ce |
| Assets/Resources.meta | f897cf109edadad467c1f53ed8b09c9f6beb23b888bd46fcd7259155b9a37cdc |
| Assets/Resources/Progression.meta | ea4ead2e8f51c61c42ab5461eb289ad295589c14705a094be42bc286b9071d8e |
| Assets/Resources/Progression/DaoAnCatalog.asset.meta | 1880e8f5bda893bf52af623e2f240412c89fe8a679200795e84b2d32b4b10974 |
| Assets/Resources/SecretRealm.meta | e17e187d8eed6c5a79e81296612038691f086f5e1d8ebd17f968b0254bb86af2 |
| Assets/Resources/SecretRealm/ChuongAn.png.meta | 40196c87e8edfa928567f5ba24696f586f8f5599223d55a6299a0b34e6dd3ce2 |
| Assets/Resources/SecretRealm/PhapTran.png.meta | ef1cdba83011d669164cdd28d30f07d77c1fb4c511e1140fa674864637303616 |
| Assets/Resources/SecretRealm/SamSet.png.meta | 91b7efa0d7abfde0d39d6110059d32656aaaa52c979093a6d0c2edc83ec9b8ae |
| Assets/Resources/SecretRealm/Tex_VFX_HitSpark.png.meta | 419bb5c3f9e72c2841c9c47b77626906ca74e6c15ab977988c1b0f7d244da738 |
| Assets/Resources/SecretRealm/Tex_VFX_RuneTelegraph.png.meta | 6ad10912077ec6606850aa5afd064a27c484d483adc1545cef461cd0d6efe555 |
| Assets/Resources/SecretRealm/Tex_VFX_SoftGlow.png.meta | bb34ced023e97856fccf1d7bffbc70dcf2c034a9ddaede089a04aa416d0be71e |
| Assets/Resources/SecretRealm/Tex_VFX_Taiji.png.meta | d2215d22453bab5cae6210c6e477524fb8f3a8d9f194b5c796568d69ae2eaa31 |
| Assets/Resources/SecretRealm/VanKiemQuyTong.png.meta | 1a4fc53139ecb2f335d131a6268bead24247df5593c688997a1a1327a5a689d5 |
| Assets/Resources/SecretRealm/bicanh_barrier_break.jpg.meta | 09a946de5fbdd528f82721e001c143b9cfdd3f2f63bc0112f743d213710cae36 |
| Assets/Resources/SecretRealm/bicanh_dan_cauldron.jpg.meta | e13c116a20c42551466b5026ddc857099d80bd524c6c1c755cc36c20d1e33056 |
| Assets/Resources/SecretRealm/bicanh_horde_survival.jpg.meta | 1a44c6f4ee739edaa65659941be9eae24f7922c919466e85ed828ca71fb29a4e |
| Assets/Resources/SecretRealm/kiem.png.meta | 6d1d610a4e3189b8554014db68ac4ff8d7906e34f84473b2ff505e5a8bd7b046 |
| Assets/Resources/SourceSansPro-Regular.ttf.meta | c6d385fc9d28427ab1e9b681fec8dd52a636bd40e1a067bf70962b6ed97b25c1 |
| Assets/Scenes.meta | 92598ef42aeaa7813a50e780bcee1ad6c832b257592d09ffb0e6fb6116bb703f |
| Assets/Scenes/Backups.meta | 34d423e89ee11f1b927841f14ccd2ace6242322aae9061e56bb33a48f5946660 |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity | f331cb2cb4eac9e16d5f7adef94d4583aad3641cf1b5d176555283bd24c166d4 |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity.meta | 5b0ded2941b99995634ca888954ec452875c85930c5bf8d1f8f39816c8062ebc |
| Assets/Scenes/SampleScene.unity | 5888cf83fb78cde7e72721ed2f5841c1322bff7b09f51a680f33f6c97b5ad79c |
| Assets/Scenes/SampleScene.unity.meta | b5815a6b12afaf810577bc89041c4ce46516c7cd6609693f8db616f8e5584d80 |
| Assets/Scenes/SampleScene_MobileWork.unity | b1fc9576ead78c964b6ee18761672199b0ec24ccbfaf104d61c676c19b6d0273 |
| Assets/Scenes/SampleScene_MobileWork.unity.meta | 5f284784b8870ae8911da425fbefbdcce55cc880d666aedda0e060ea9ee9fda9 |
| Assets/Scenes/test.unity | f1ddbb4465bd28c7d268b4df4dfe68ec2fe0b437587e31c7a5ab10a41654dfae |
| Assets/Scenes/test.unity.meta | 8eb823f7adc42d98beea6e821e407f2afc267604cd5d41ef5b648f9c61fe8494 |
| Assets/ScriptableObjects.meta | 2805d1d8d1c5d96177878cc83cf658dbfecf9c1c1f8ceb84a6f9f79282e59471 |
| Assets/ScriptableObjects/Skills.meta | dca73d6a718b369d7d74b6b4d81fe3c89512f70114bc525effd39fc3fb38c30e |
| Assets/ScriptableObjects/Skills/VanKiemQuyTong.asset.meta | 6becf5e7057ba8d5a5b751fea2b9cb20321c9529f84a80963cc6d1a770600350 |
| Assets/ScriptableObjects/Stages.meta | 2b983f8e16a1d6a03c4f7b57b55f850a51aea6377be577749fa18b2ffc0fbd2c |
| Assets/ScriptableObjects/Stages/Stage_01.asset.meta | b31a2ef91331cd8852d5b46e02af1692788a35320c445cc9a440a58329fef807 |
| Assets/ScriptableObjects/Stages/Stage_02.asset.meta | 65c9a0d855c6f7a01cf6224417a106b6761f43fbd9c322c0b44e5ff107d4ef0c |
| Assets/ScriptableObjects/Stages/Stage_03.asset.meta | 762fc4d6c2e92c79d44aba7c6753841766d389a51a6c3e371c92b4651471ef7a |
| Assets/ScriptableObjects/Stages/Stage_04.asset.meta | 4bf5fbe9751742655a084db8147f44bf755a55f141a47c6d633eba14a12a6c29 |
| Assets/ScriptableObjects/Stages/Stage_05.asset.meta | 4db7829c17392d8ddddbc4e451c36c301e8f46f27d98df72e0123ee10c275421 |
| Assets/ScriptableObjects/Stages/Stage_06.asset.meta | 1d4a3285c283e53ccbb3ea91f6db934f75653e04cc870c51c796d5cf2690ba70 |
| Assets/ScriptableObjects/Stages/Stage_07.asset.meta | 8b9751af5aa50646861b7ecfbf33665f01837297b27a17e4bac2a87ffb8ba49f |
| Assets/ScriptableObjects/Stages/Stage_08.asset.meta | a50a0eab8f27634f12c5fceb4b9b10a25c8ff28347124a6195bff101087c2e35 |
| Assets/ScriptableObjects/Stages/Stage_09.asset.meta | f968de8f58b9e46b45d0e708414b925d48518e9f45715f685c803ba5f4304030 |
| Assets/ScriptableObjects/Stages/Stage_10.asset.meta | bfc73d7977714a8a08caab59aeeb2953bd29446df0c7183764264dd4775ddf10 |
| Assets/ScriptableObjects/Stages/Stage_11.asset.meta | dfa0afdba8b76d6925607699f0bfdab60d4dc20c8ac22c923e640ba03d87d575 |
| Assets/ScriptableObjects/Stages/Stage_12.asset.meta | 53f363c3a10584240ac4171ef86d9739d9ecf639d9dc423f1802cda1a4295263 |
| Assets/ScriptableObjects/Stages/Stage_13.asset.meta | 4fdcae88cce7db4e64d2219a62fd54a4f203392019511cbd099b98174ca83095 |
| Assets/ScriptableObjects/Stages/Stage_14.asset.meta | 2185863a2706114bf413f4a34ce62d74bd054de4ca054572a9c170e7635e3231 |
| Assets/ScriptableObjects/Stages/Stage_15.asset.meta | 9a8d35b15ddd2679b23f639c281e87ddcd858f59f774fa353e2734534c22423f |
| Assets/ScriptableObjects/Stages/Stage_16.asset.meta | 737db70409e37a9f37510925d30b8115d05db41010315e8133559c48729c0bbd |
| Assets/ScriptableObjects/Stages/Stage_17.asset.meta | e0f4daebfad1cf2a24b7d02457691291c339cd847f7e32dbeafd455411ed1562 |
| Assets/ScriptableObjects/Stages/Stage_18.asset.meta | 67ad87886fc82fef5a22e7988c4314e0c0999adde06b7038ec565b7d28022388 |
| Assets/ScriptableObjects/Stages/Stage_19.asset.meta | a1734533df260a1ddeef481e62ecce3eeea9e2bff906429410193bc6e38de8a7 |
| Assets/ScriptableObjects/Stages/Stage_20.asset.meta | 76f3a1237dcd9f9a6c153569588d5c14d81f2a99246404f77fda585059e01ac7 |
| Assets/ScriptableObjects/Stages/Stage_21.asset.meta | 8e49fb01798ce121299a26a4174f02cb8004944d6ca6265695cc13af4e30fd72 |
| Assets/ScriptableObjects/Stages/Stage_22.asset.meta | a33b7bf9f95b765f50a9416790f5298d6f55d8253d89eac6263da4a6ead74d3e |
| Assets/ScriptableObjects/Stages/Stage_23.asset.meta | c22686d7accdd0c5e1ac7b34638b112731595ca7e1c940e746fdef3f7795f7f9 |
| Assets/ScriptableObjects/Stages/Stage_24.asset.meta | 12946b0d88fc273102f498c1bf1b054a5a2ea54e81ad1f9d048fd5b6b2809225 |
| Assets/ScriptableObjects/Stages/Stage_25.asset.meta | 6f414d1448cf5aa825d54ced1c8a510d3299ac15c763299e751aee9d96388a9e |
| Assets/ScriptableObjects/Stages/Stage_26.asset.meta | 2f2b1a7b104c57577c4d87febd079189ad3902f9080f4ecade7fb4a1d0a4a5de |
| Assets/ScriptableObjects/Stages/Stage_27.asset.meta | 319e8c24f4a67f8d3ff0862fc0a2ebe919d87cb4486708884228f6759c61def2 |
| Assets/ScriptableObjects/Stages/Stage_28.asset.meta | f74faf15eb514393b3c758ac71307fed5ece02ecc8829eb9cbd158906049e038 |
| Assets/ScriptableObjects/Stages/Stage_29.asset.meta | 58ee38cb712252a2d8ce52da327296f89827f9b4fa43404a65475b7d056a7db7 |
| Assets/ScriptableObjects/Stages/Stage_30.asset.meta | a3f574375d11e52277a13273d9344f7232dcb7775002f79ee50aec000076d79d |
| Assets/ScriptableObjects/Stages/Stage_31.asset.meta | 4c68dff5f50c8f2e3377a283433958e1bb494c85342c30d63e26634da221c526 |
| Assets/ScriptableObjects/Stages/Stage_32.asset.meta | b71939366eb98fb7a8e5613d241cafe1740ad6ca9878e404736f4750b8c10d80 |
| Assets/ScriptableObjects/Stages/Stage_33.asset.meta | 0825ab667aa145995e6239de1c418e1511f3b4b2dedeb3a17d1cba8b0d42787b |
| Assets/ScriptableObjects/Stages/Stage_34.asset.meta | 845d92fbd2b3a483d1178d596b6148764be09c47f022b2bdc5d10803068b2bb6 |
| Assets/ScriptableObjects/Stages/Stage_35.asset.meta | 2b531803dc9ec96ffe35d3107fab68364848170c391f07ba35fbab91582d7cbd |
| Assets/ScriptableObjects/Stages/Stage_36.asset.meta | 383c99e3b4c83182d60244e4d6857662968e141c6a9ecb6e820b6b43b9d9a880 |
| Assets/ScriptableObjects/Stages/Stage_37.asset.meta | 6ae76336faad57f77137d92e99e654f576e1c6da3b35b490f86f9dd5509c5f08 |
| Assets/ScriptableObjects/Stages/Stage_38.asset.meta | 6482dd442b5c964fb2eaa9d094817b27190b4fe276237fe4c050eb32d4df826b |
| Assets/ScriptableObjects/Stages/Stage_39.asset.meta | 8044512d7cadb2754f3803e53c9ad40693e4151c3f39c912faac2c6a6e57ec61 |
| Assets/ScriptableObjects/Stages/Stage_40.asset.meta | 3382c608bb92750c795ac40a6ad81ab903a7809fe9e44f47220470771d21e449 |
| Assets/ScriptableObjects/Upgrades.meta | e7f6fce975aef1a5f984c9be3c6e916813023d0f5d977fe5a4774be64b65221f |
| Assets/ScriptableObjects/Upgrades/KiemY.asset.meta | 84e025f6cf102c3212691eeb02c38c7e22e22998f512b27f33cf11f79f0b5213 |
| Assets/ScriptableObjects/Upgrades/LinhLuc.asset.meta | d602dc8f4dd9874ea6868a615b96abc6a143cc6ac81e3a5645643b37396edb6d |
| Assets/ScriptableObjects/Upgrades/ThanThuc.asset.meta | 7754508a10cca0dab5c64947e53b6d456f9b7a841d4d36243f13c014a966b0de |
| Assets/ScriptableObjects/Upgrades/TuLinh.asset.meta | c4747fe7b4351d18484b358642e7ba230882a13b41509ee8eea31f012b0b9868 |
| Assets/Scripts.meta | ecf4d3b0b79817f4ce1e2d0fff46c571c77e81bc29e82efc307a641ab1e7f631 |
| Assets/Scripts/Combat.meta | 925d48629b958a4ea7ba0b8ae077aece83af737fd7dda41c6b9ef73fea87ede3 |
| Assets/Scripts/Combat/AuraVFX.cs | 447c94c0849edacd039c172cec87e6b7f739a1c787d060e1480f6f7480328443 |
| Assets/Scripts/Combat/AuraVFX.cs.meta | 3e1cc566cff9f8c6d43ab088b40a716e0e6e3dc6cedb8219f64d91e17713d15c |
| Assets/Scripts/Combat/BossDamageVignette.cs | c3d8a3b8b7927a0c01c9fba01c25993492ba2f9ed671f25eea3a571e53f7e3a1 |
| Assets/Scripts/Combat/BossDamageVignette.cs.meta | 88e67439feb6893779b3628ed2a12b9eeea408bf4971eb3bec300fe27463ce64 |
| Assets/Scripts/Combat/BossEnemy.cs | 07a3bc7f8a81a6645f913a1f042aec47d963dd04a2daf3e3a06c596138de1e4f |
| Assets/Scripts/Combat/BossEnemy.cs.meta | 21bdb8cebd42e4b85e954c51c27b3b3042f4dcd319840e0189e674c64d9610a2 |
| Assets/Scripts/Combat/DamagePopup.cs | 20abb024dbc10c1bdfa9b66695a7ddb73ae599c50984739944a7ac16236ae811 |
| Assets/Scripts/Combat/DamagePopup.cs.meta | e3e98409325ea1d172792e8401d42e1d15b68d2daa585024d4a84cc4d2fac36c |
| Assets/Scripts/Combat/DestroyAfterTime.cs | 0f4a2f0f57015adeb305dee7695bd2036c6bcf1f76684e5b5a21a7599a345663 |
| Assets/Scripts/Combat/DestroyAfterTime.cs.meta | f631bd91c4f39e2b3a107dd9186c0112c131a02ecb9794e9be9e0b2598dd2e4f |
| Assets/Scripts/Combat/DynamicCameraFrustum.cs | 49a87620cb1097eda0e77e8a0de3f32a9239c3d74e6c07e00e58b1f766207b3b |
| Assets/Scripts/Combat/DynamicCameraFrustum.cs.meta | 1dbf97e62b0156127b0feb91dbbf88574415ce1228007498cfbbad7534fdfd9b |
| Assets/Scripts/Combat/Enemy.cs | 8953035a231eae3da94b5047686dc33fd3624890ee0163aa4418887b3bf9ff69 |
| Assets/Scripts/Combat/Enemy.cs.meta | a0c074efe4372d3ce2f18d7a2741a44cbd0f14c075e2e2d54e022bde905fae0d |
| Assets/Scripts/Combat/FlyingSword.cs | 80738fe959f31dc9f4f79bcfbbc65375c7b7810591b8a8e660a0bedc8338290b |
| Assets/Scripts/Combat/FlyingSword.cs.meta | ff65b45127af5a43b8c43e5169e8fec3b37fc366bf7c683b73cb5c09eda7c227 |
| Assets/Scripts/Combat/FormationGlyphVFX.cs | d721c0bbac110696aa1eaf1081989587d5e26bbf0018b0eac53ec5a6a05ffabe |
| Assets/Scripts/Combat/FormationGlyphVFX.cs.meta | 7b35a70081a725b2c96f082e22626caad6662e89001933fc7ebebfc568f39653 |
| Assets/Scripts/Combat/GiantHandVFX.cs | 1ccc03b5503d5d7abc0d764e10e1418e8ebe4d6fb5cd00c82d2bbb3e628ac6d6 |
| Assets/Scripts/Combat/GiantHandVFX.cs.meta | 8efdb716cf467e0f6074fb3eec54c7a6841cddc54884957df29b1df059c33ca3 |
| Assets/Scripts/Combat/HitVFXManager.cs | 7529d54989f974b71055d006bf5836eceb5d931b3be6a2ba73fe0daaf4e340de |
| Assets/Scripts/Combat/HitVFXManager.cs.meta | 89bee4d6fd02e5c88f6ea0f94acf3ebc5733553c069dca45e0d36a7fbe2a4bd6 |
| Assets/Scripts/Combat/LightningVFX.cs | 4f4e4612b25f62d5c1a81b458b961111dbb73827743854a424cc49c14a0a402e |
| Assets/Scripts/Combat/LightningVFX.cs.meta | b111752695c1c51e022d23a3fb498ecb4e3cefa139394e2119ac673ccb69a492 |
| Assets/Scripts/Combat/MergeItem.cs | 03870ba7ac228a624e150a38619707c3db441c4331ea0333550fd82843aef894 |
| Assets/Scripts/Combat/MergeItem.cs.meta | 7aa8dbb7c52f634b9542c81c6f5c9e8899b929590098c0148a826cba62a62a23 |
| Assets/Scripts/Combat/MergeSlot.cs | df8c989a013c27546df3475a172aad2e79853651a71700e1b4c438faee109ce2 |
| Assets/Scripts/Combat/MergeSlot.cs.meta | 47a5eeeb63099c59d670c888524c9d68cb2a1cf69f1399294ee8677998801cc2 |
| Assets/Scripts/Combat/OpeningBossGuard.cs | 72ede622ee42112cb8b1b9c12958a448b7aee5258485a5e256a2a4fa7fc185e0 |
| Assets/Scripts/Combat/OpeningBossGuard.cs.meta | 5e3af9da27a013bfff7f391d1fc43f5a11b284e5c13f2a2ef8c126c75b5b4d56 |
| Assets/Scripts/Combat/ShockwaveVFX.cs | d322d1be6ce4ad3b0e648501f51b6e3980c3b5b2d89525e9fc1b2fce046e4f63 |
| Assets/Scripts/Combat/ShockwaveVFX.cs.meta | 4f4d90b5468ccaf899a3ac9d21bbc00f3240b5f15f793836ab011aa388e4d41d |
| Assets/Scripts/Combat/TribulationLightningEffect.cs | 4dfc59538fc4336c8a8b32d70f58b1956404a68c955ee4e40cb56cf7514270d3 |
| Assets/Scripts/Combat/TribulationLightningEffect.cs.meta | c006dcbf95ac05327bebed5982c73a6c21ad318c16e6d082afed83a05dbebaef |
| Assets/Scripts/Combat/TuTienCloudSystem.cs | c32624bf0fbd012ee83239720e7e7b972bdf5506e430add3cb690f33bd0db58f |
| Assets/Scripts/Combat/TuTienCloudSystem.cs.meta | 7ee606664dba3269dd2dba99a42a0f3ffed5653baa6676eb2e6ddd1587b12efb |
| Assets/Scripts/Combat/VFXHelper.cs | 80b9e26245b6d2f1f22fd248558123a8cc35633d6afa0e93e992f41e872ea767 |
| Assets/Scripts/Combat/VFXHelper.cs.meta | dbfec4d99ae7357b1f09b375934badd418df4947f597d53cc88b2000b36e8f61 |
| Assets/Scripts/Data.meta | ef6dbd046e30436f7a8648a554e65b69d3f9c56b618faadd96447666e64757ce |
| Assets/Scripts/Data/AchievementData.cs | f03375217b1531aea0d396a07c38bc796cd08a02533ae608de27c7752083eb7b |
| Assets/Scripts/Data/AchievementData.cs.meta | e90ebf2bf49353fcd34ef029e67d3de2a8c1c1261730880522c3b60dfb55ab81 |
| Assets/Scripts/Data/CultivationDisplay.cs | 583aeb99f92ec260acf5416a0ad4aca5d9d7b47a7fba44e8a8eb5f1e16427ef7 |
| Assets/Scripts/Data/CultivationDisplay.cs.meta | 4e8370fa525c521fa6b528aa95ce0d2d6edf424eb11c484b330efd9b4693f454 |
| Assets/Scripts/Data/CultivationStageData.cs | ea39f0562b69ac57288b3784ce51ba0a2d51771561b4901c9b08465229674c37 |
| Assets/Scripts/Data/CultivationStageData.cs.meta | 2b548b053f1d9f6ac36ea2814e7f3e0d53559dcfe54df72267720cada756fd79 |
| Assets/Scripts/Data/DaoAnCatalog.cs | 02c8a0f9d108817186e755021b4daaa5a5919877a461f33d2f9fccea9650ab10 |
| Assets/Scripts/Data/DaoAnCatalog.cs.meta | e07d61081372a13f9eb76c8de42d74699f2e8be7f7ec3a047986a3658e806105 |
| Assets/Scripts/Data/DaoAnSaveState.cs | fa6f45ee764ca2f4e5f279eb934d57d9e1b42bc7f565eece9f34286065a4fcba |
| Assets/Scripts/Data/DaoAnSaveState.cs.meta | f74ac7a7690e5c5939d3c8f70ef2a9d3e0956eaac5866f747fb7c33e74a3d9c4 |
| Assets/Scripts/Data/DataManager.cs | 676250c99b898eb8be28521f15b129b2c35a74dc76fc9af998eeed8c8b207ca3 |
| Assets/Scripts/Data/DataManager.cs.meta | 1608876ca5de5811bedac3d759cf63baf54dda81a4a40a09aed275c2ac4b9525 |
| Assets/Scripts/Data/DeepProgressionSaveData.cs | 2acb220804e61b8719b6d98bf40e24e7d00b02cb58c8c5a8b07dc4a732309c17 |
| Assets/Scripts/Data/DeepProgressionSaveData.cs.meta | 06c75214e9a55df860587467898b5997f1d41af5e953d23344c8dd686d25b3da |
| Assets/Scripts/Data/RealmId.cs | 99e32bcbde4055b084a8945dcd64901025e115c1c65b794b6f063200c33c3428 |
| Assets/Scripts/Data/RealmId.cs.meta | 97b5752888303e0e52051dc87f8e778c262cfeaff6a384b2c6cda382f39fd2d0 |
| Assets/Scripts/Data/SaveMigration.cs | 592deaf3cc5b5451c36fbfff20dea4ff8093bdb9915d2f7dd811f390e0873bea |
| Assets/Scripts/Data/SaveMigration.cs.meta | 71a2871912848e3b800fe475ee20791bafec5d7fe94a1e5eeac6f06c2c426107 |
| Assets/Scripts/Data/SecretRealmSaveState.cs | 88b766ff19769d98f4f511e5f7d21a0957111f3c32d673173aef2fadcb0fa87b |
| Assets/Scripts/Data/SecretRealmSaveState.cs.meta | 1b4ca30d9655c6d849a12c75b3d7938834406d023cc64f30caba58f77948812c |
| Assets/Scripts/Data/SkillData.cs | 9152f3aaef290b07d2dd12e5863a5b2b7876529e0afc4968a41a68e4baf3678b |
| Assets/Scripts/Data/SkillData.cs.meta | b23c148d06c4d7c671b5bb98ed7125b89061d58f9f2a112900a51f92ccf7c188 |
| Assets/Scripts/Data/StorageSaveData.cs | 780819b3897ea62a2295a7544b5be33fdde31fda08d442e2d3054cba33b2bc88 |
| Assets/Scripts/Data/StorageSaveData.cs.meta | dbce2d34a0be279f508b870f14f9dc88b68cbb1d57d5f63e0459e694797dbe81 |
| Assets/Scripts/Data/UpgradeData.cs | bfdfbdf1fb6a22a0f99555c057f57ea352a708ebf7fa2ced3b52b753d69d0a2b |
| Assets/Scripts/Data/UpgradeData.cs.meta | e5aad80ea4d11dcf0bc1017fd59b46a64acdabf63d276e15b58d3b11160d5872 |
| Assets/Scripts/Editor.meta | 58e1150c770343d46d40be8b607c7d89be675cc746833bcee69ff2edb93b7324 |
| Assets/Scripts/Editor/AchievementUIBuilderTool.cs | f251ef82dfa1933791fbfaf3bf431c80d7006f335f85fb965771ef0f2824a2d0 |
| Assets/Scripts/Editor/AchievementUIBuilderTool.cs.meta | b39765a7d03adfe72dc6608df358c5535b3088b8ab32ee22f2c85d7530c87132 |
| Assets/Scripts/Editor/AudioSetupTool.cs | dbf5f2f009e7d7f7388a82d93b230ff34443920b1f25df999af49f0d6f992d2d |
| Assets/Scripts/Editor/AudioSetupTool.cs.meta | cd680098cbcdf5e099366f58a67d0014863e630bc91bd1aba0fade258f9b68fb |
| Assets/Scripts/Editor/BalanceDataTool.cs | 69f6808b14e27c1a0924fa8d896cb1a2cb4bb245e9b3d5a02823b5813802cac3 |
| Assets/Scripts/Editor/BalanceDataTool.cs.meta | 6ecba7803eb9b2af85c10d0a74bf0f82e2c8f8870766f96952a5edbc869f370b |
| Assets/Scripts/Editor/BaseDataGenerator.cs | cf1907030eecfae778e520e9e2be232a0362c2c1608031c241c1ae048c9e95b2 |
| Assets/Scripts/Editor/BaseDataGenerator.cs.meta | 70a17a7f35cbc1e7617976b80ab53f6ec78c215f22fe9c0964dc419554d7b3d8 |
| Assets/Scripts/Editor/BossFixerTool.cs | a29b8fa87312857904b258cfe9f5e9629688011577c4aacfdcab62772da62ea9 |
| Assets/Scripts/Editor/BossFixerTool.cs.meta | 8336ee638b121d0b616af49ee5df279a52496fc343710a70be5b7a341b2ec6d3 |
| Assets/Scripts/Editor/BuildSettingsChecker.cs | 2afeddb6c3b488508dbefe58e1711428c7d2eb7dd43573af0d6d96831267650d |
| Assets/Scripts/Editor/BuildSettingsChecker.cs.meta | 49baf62110bb8329d6df0e9e2caf10e8e363549110ecc04efa448eada93d633f |
| Assets/Scripts/Editor/BuildTuTienMapTool.cs | 2e6d5345801ab2fa8e4ddd75360c433e4153280139a2b803d0fb6a3c000ac9cb |
| Assets/Scripts/Editor/BuildTuTienMapTool.cs.meta | ef49a24edb69dcf8f22e9ea4bdd8f9cca664e490bce5263e336e3b6265a5271f |
| Assets/Scripts/Editor/CanvasFullTreeInspector.cs | a662b16f03534d2e3ab03ae505b3e935cf6ecc55e178ac5c46807ab386024eb4 |
| Assets/Scripts/Editor/CanvasFullTreeInspector.cs.meta | 4429798fff0f30e4c1f4e292fe6b733799b8492053f5660a39627d0630cdead6 |
| Assets/Scripts/Editor/CanvasHierarchyInspector.cs | 5f222dbe0d5a5ffed3602825ef7799413d8b7b359972bf4baf7f95cc4e210041 |
| Assets/Scripts/Editor/CanvasHierarchyInspector.cs.meta | 06d3b6ee281d284ffb1c103faf149c209b340a8a94ed66e18e1186501a87f231 |
| Assets/Scripts/Editor/CanvasInspector.cs | 62cf3ee47e04260fe1589b5ae24f35bf3282bf5bee7c46b4af4d5474095a2ec7 |
| Assets/Scripts/Editor/CanvasInspector.cs.meta | e862a83433f52a50240fdfdd1097f87e3ba1efbc0a0987e8c33c042a1c78fb45 |
| Assets/Scripts/Editor/CharacterDetailBuilderTool.cs | 8a5a1e11577223b88e77dbec74ece70fc415503ce053e63a8810fa5f753a8426 |
| Assets/Scripts/Editor/CharacterDetailBuilderTool.cs.meta | 56ebfd76fad7cb1ff87be916625c10b462dd13cd7852ea561e78c6c7af49eed1 |
| Assets/Scripts/Editor/CheckActiveCanvasObjects.cs | 90569320ced760773f6ee4825016c14dcd5d091e5ff50ccdd3ece8c61b942dc6 |
| Assets/Scripts/Editor/CheckActiveCanvasObjects.cs.meta | 733bb98aa66a40472f2820047803e1498e68a87226746e22d61faddb5640cefb |
| Assets/Scripts/Editor/CheckCanvasScalerTool.cs | 3ea2c391ca451f0a6c0a8ca30b8c7e90d4b88430f6ae7250a6f82b250db1cd22 |
| Assets/Scripts/Editor/CheckCanvasScalerTool.cs.meta | 345298401253ddc7039ea997dadf861d6fae10fe5bd602923eac90dcb2169786 |
| Assets/Scripts/Editor/CheckHPBarSprite.cs | 327f5d654960061bf2f43d2c140b98f3a90504ee1290c19e3df9b44ec99083da |
| Assets/Scripts/Editor/CheckHPBarSprite.cs.meta | 8631234900ad2fb568334d20d09b63ecedafe062ef9784080a8e23af0c7cbdcc |
| Assets/Scripts/Editor/CheckVitalBindings.cs | ce63ef631b5c9d055fa5b0c2cd5963a2d05a7df424e1e9278a29a0db5f3541fb |
| Assets/Scripts/Editor/CheckVitalBindings.cs.meta | 8aabac3b0097bb92f0cab6668e332494adf40e0c63bc8db449869c08dbbd5a25 |
| Assets/Scripts/Editor/CombatBuilderTool.cs | dc24d2378f1e75c8d72814415752b2919febe50f4c6f69c6ed583fe725e97bef |
| Assets/Scripts/Editor/CombatBuilderTool.cs.meta | 8fd00ce904ae162e6acb95b02c0680b29eaa9fcf7898ca7c55eb13b6114081de |
| Assets/Scripts/Editor/CombatNotificationBuilderTool.cs | 17a93bbf5c680f883bfa45666aea6a84a80f40a205d367b5f9be9d8f164b108b |
| Assets/Scripts/Editor/CombatNotificationBuilderTool.cs.meta | e7c817dc3e9c15aea8d2379fa182b856afad6378c98cdca0fe4c88d8703eb233 |
| Assets/Scripts/Editor/CombatUIBuilder.cs | 100b0468393e1bf7d09d033c36931c9b397a55d6493bd067d39215d8ad3b3d06 |
| Assets/Scripts/Editor/CombatUIBuilder.cs.meta | 4bbd5968734a7bd3bfe4b1d0664085f4534079f110382824635c9ad426950652 |
| Assets/Scripts/Editor/ComprehensiveCanvasAuditor.cs | 6ddfb9def6ca3c8cac84f1911f4fe6269634f237bcb0873d8093602b289ab3af |
| Assets/Scripts/Editor/ComprehensiveCanvasAuditor.cs.meta | be18cf5fbdeeb7190fe4fa5efe0dd335669f2f24aa86573a3f00b19afccbb211 |
| Assets/Scripts/Editor/DaoAnCatalogSetupTool.cs | dc1d45eac1c661bed04b011645f5f7541ef9d01326956f18b88313e998b617cc |
| Assets/Scripts/Editor/DaoAnCatalogSetupTool.cs.meta | 4e654218324bc17231b3c39de8a384ab1fc5e4b5ad79bbdda9c17d5d896755e6 |
| Assets/Scripts/Editor/DaoAnModalInspectorTool.cs | 18f88b3b6661efdc1a90572b4f646b8ba2104fd0bb6de21af09301fce290f295 |
| Assets/Scripts/Editor/DaoAnModalInspectorTool.cs.meta | 7629d4f2ddff36ba7494ae0b215d8462c1a930bdcc380de44e0242a22b6a3e21 |
| Assets/Scripts/Editor/DaoAnUIDebugCapture.cs | 16986071e4eb9fdedd7058ae6b2c70ea7cbfb5be0774473d237ad93064fc73b2 |
| Assets/Scripts/Editor/DaoAnUIDebugCapture.cs.meta | a45f846693c8011508bac973b42d819b4f009cc6d2d59523b70d731e3a10ebe5 |
| Assets/Scripts/Editor/DaoAnValidationTool.cs | 67c6528085aad2aec05e01007c22a7c8e7d37c39cc743051983393b632921237 |
| Assets/Scripts/Editor/DaoAnValidationTool.cs.meta | 7667515b394f6dfaf1135043f37ad1972a436ce4eb5a151b5b604b3fe1fa96cd |
| Assets/Scripts/Editor/DashboardBuilderTool.cs | f4059515624676a12f919762841b1f5a0c131d1c8f601afc2184b118d469e7e0 |
| Assets/Scripts/Editor/DashboardBuilderTool.cs.meta | 52509d6d589439ddb2b08cbc45f0d001f80a1ab3dc58fdde7f61f40ab96f8dff |
| Assets/Scripts/Editor/DrawerDebugHelper.cs | 020e62c13cffacc3aff63920b562008bd042c937c8364e2c88599fd8b5e23d6a |
| Assets/Scripts/Editor/DrawerDebugHelper.cs.meta | 2f5983d12aea3305215eff1fab910636bfa09848944db3f28b82204954c9caf9 |
| Assets/Scripts/Editor/EnvironmentBuilderTool.cs | 4c5beeafff2f64560fc23b9358525f7ba76d81759b4ed214f998cfe58756bafd |
| Assets/Scripts/Editor/EnvironmentBuilderTool.cs.meta | 9ee4f4d6cdf97d64bbbe77a09a550fe5dd4a5218e198f88304cdd4cd6eb130be |
| Assets/Scripts/Editor/GameFeelBuilderTool.cs | 52e5e3843eceabd178e5d41c26fbe3594a109ba5ade5425f2f34e9fedfa38bf8 |
| Assets/Scripts/Editor/GameFeelBuilderTool.cs.meta | cca65477d7dc9fe8769f97cbcd3dae49d5aaff57a638a9b7afd577d268185650 |
| Assets/Scripts/Editor/GameLoggerUITool.cs | 03c6f124fa97577d651c87f23a0f08d8db0991aed57a4c0d0927fec597bcca36 |
| Assets/Scripts/Editor/GameLoggerUITool.cs.meta | f2ba857752f1dcdf122252d694d5e4c31dca284a644f78143611abba93528036 |
| Assets/Scripts/Editor/GameplayAutomationBenchmark.cs | c6c4d8334b16ca6f164c2443dd26a89468065281f6d9ea2fcba7e925d6defe71 |
| Assets/Scripts/Editor/GameplayAutomationBenchmark.cs.meta | c6f58aff7b98f3d345f91bfe0a6781aa37895d40cc1b2dc1581ae18cc9c08bb6 |
| Assets/Scripts/Editor/HUDInspectorHelper.cs | c7d40bcd87a32e0c19b20d1bbc96b8b124a2ef1e4b08f0ef5294f93118aa5657 |
| Assets/Scripts/Editor/HUDInspectorHelper.cs.meta | 86ec50bb7718ce0d44efa8638867054d76640f33b4d5771b523e67788e8d9513 |
| Assets/Scripts/Editor/HUDLayoutRefinementTool.cs | 0a74863935073e9d144bfb3a23060c5c0b8fe1e8eaf8d892d4d5bc4865b25e77 |
| Assets/Scripts/Editor/HUDLayoutRefinementTool.cs.meta | 44f8bb222b8671c2295e7d14c9f17ec744b9c8529aec5077b51beda542933439 |
| Assets/Scripts/Editor/HUDModernRedesignTool.cs | 64f1db2015d9fa7431fd329bc39b85c0aff4c75afe3093cbb2581ddebb83939c |
| Assets/Scripts/Editor/HUDModernRedesignTool.cs.meta | c2112108ede226c6ebcf3b7bbc18f642d0a0357a9783782d78273548bfe587c1 |
| Assets/Scripts/Editor/HitVFXTestRunner.cs | f7089d69b743fe0e9812bcd2405889a765e252aafc1aa7855e48916931f53041 |
| Assets/Scripts/Editor/HitVFXTestRunner.cs.meta | de8c48ec1758adb0cb42973dab08168749a6d6c9235e4594e9a8f919180ae574 |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs | e2556c05d019773bbfcbc94a8e0f9fb53c92c56af599e95b6d4ff8cfcf6b6ed4 |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs.meta | 1976bb188e1efa507426a29a1f7f232c0c575636c1d982a411166ed5e48539f6 |
| Assets/Scripts/Editor/InGameUITester.cs | aadbc77c0defd9535c86628594138c495fdea4fa9bda7284fa37b9e24cfb5dee |
| Assets/Scripts/Editor/InGameUITester.cs.meta | 7925bfe08426fbac2d9013397a8f3946040489fb94b7285af4028402eeb2e972 |
| Assets/Scripts/Editor/InspectGameManagers.cs | de41dd7004ecdf411ef55b133f6e587017c1ab06fb45b26733cfd795b9539d49 |
| Assets/Scripts/Editor/InspectGameManagers.cs.meta | 4e167ee89baaec3d0deebd1d078b0af101133e8981616a0d529700aa0f43e0da |
| Assets/Scripts/Editor/JadeUIRedesignAudit.cs | 27209e4e5418598c16be54ddfe8716b71cbd9a4fbf9640d535337297b06046c6 |
| Assets/Scripts/Editor/JadeUIRedesignAudit.cs.meta | e4cced8b4ec98de8f123e0a227113b221fa05e6204ddfa6d41933da95283c166 |
| Assets/Scripts/Editor/JadeUIRedesignTool.cs | 88daf7d06827e2083423f41e7b6f348640db22152f421cd42926707d5080a496 |
| Assets/Scripts/Editor/JadeUIRedesignTool.cs.meta | 051c991522be1930696a7f9ee2f05a89889f1813bc4f0d767f51d38681a252ec |
| Assets/Scripts/Editor/JadeUIRuntimeChecks.cs | 9613cdb4bdddc5fbe4b2ffda5c107f21a217bff53cccbe986f1278e7817d7c0b |
| Assets/Scripts/Editor/JadeUIRuntimeChecks.cs.meta | 7b78a54d6ad89267015c83980493bdf62bb213191b544c95bc7befddf0bc7bde |
| Assets/Scripts/Editor/LegacyAuditTool.cs | 11f529804618ec4aa589613d90138b0af48a44708e1a247155ee3415a933abc3 |
| Assets/Scripts/Editor/LegacyAuditTool.cs.meta | 8626ddb44c8c54ec5f00b06625c87d7cfefa70a13a45edf4507598cce0792837 |
| Assets/Scripts/Editor/MainMenuUIBuilder.cs | d111b7274840773c4eb868f38a69b929d0b976d859c66e5fa0c166d3e31e8e0f |
| Assets/Scripts/Editor/MainMenuUIBuilder.cs.meta | 15e7c6d59e599ca4dbc7c22c5dee5ed396ccbfab6297cf045b8703447f4c5198 |
| Assets/Scripts/Editor/MasterBuilderTool.cs | 5ddb8679b17110327095df3d996450b510b8c88daf03f84a55afb7a80063d8cd |
| Assets/Scripts/Editor/MasterBuilderTool.cs.meta | 9867a6ab467798010da01522dfb803700670c5fc671957d3573cc415591d1fc3 |
| Assets/Scripts/Editor/MaxLevelAuditor.cs | 5bc87c8a17304d16f65dce1a5db6aaa41f17b3be2c278fe3cedeec405f544e8c |
| Assets/Scripts/Editor/MaxLevelAuditor.cs.meta | 8db2db2803a750334b38219ea4a49dd359d4a70e0530422d0b04302f3d3f3560 |
| Assets/Scripts/Editor/MergeBuilderTool.cs | 965f6ca15a1e1eb31a8c0bc0f2cfe1acc16967017bc4c9e45afbbcf53959c065 |
| Assets/Scripts/Editor/MergeBuilderTool.cs.meta | db254f83c5e1e1ff7a7b629d77afcbb64f161b999f38393cbf7e523d67a24ab2 |
| Assets/Scripts/Editor/MilestoneUIBuilderTool.cs | 8ec219aee90f86e99686ec82403a3bccfb4a905b4e8d9f3324b4cead5214b662 |
| Assets/Scripts/Editor/MilestoneUIBuilderTool.cs.meta | 6f153f1d150c711245a0d7cdf404480452af8963716d8df229e6f3e0e14390ec |
| Assets/Scripts/Editor/MobileDeviceViewTool.cs | 9be15c920aa8becbe3cc91fde87af740d3c16c1ab5cdf5803d87c01de06caf67 |
| Assets/Scripts/Editor/MobileDeviceViewTool.cs.meta | 54b04c0a7055a8df0e1d9102415244d085a4c561707ff1c4279a71d5cd57b464 |
| Assets/Scripts/Editor/MobileHUDLayoutTool.cs | 8ddde4375a0f62f85b72050cf8d7ebebf0f9772114452d833cd427dc404da608 |
| Assets/Scripts/Editor/MobileHUDLayoutTool.cs.meta | 351848830d5046e661c1d49f8b7968b33d35395fd004fe2a33f6a51a2419cc1b |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs | 85e47088331b7e61f47c2de1e2320003fe9558331ea6e00a537430751c976194 |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs.meta | 630144e503ba8ac0fc4d3dfc13c36102dce5fd75cbd79735ee86698e0f6cfb40 |
| Assets/Scripts/Editor/MobileSkillHUDBuilderTool.cs | f01e3b81b2c5def090fb0721cfcb7ddf9a255d6b120796a145720cdf8f06e353 |
| Assets/Scripts/Editor/MobileSkillHUDBuilderTool.cs.meta | b9822d733a87ae74eb909cec4e0bc70b585a3d422bab5b07ff62d2e111c67a6a |
| Assets/Scripts/Editor/MobileUIOptimizationTool.cs | 328d9d854f3b41e34da7b148923f7e496c681a6eac13ef58f1b23193be977358 |
| Assets/Scripts/Editor/MobileUIOptimizationTool.cs.meta | 2fcd37705dc2daae39ae49c68b59516a1a5ada31616537fe73e74771cf03ef9b |
| Assets/Scripts/Editor/ModalInspector.cs | c9488c72a0aee822d438e728818dd038ff7b66188549a6d700b68cf80b27cb33 |
| Assets/Scripts/Editor/ModalInspector.cs.meta | d97a86e78bbdaf12e178a34af26a52f2c30b39b0e9e88ece4ca5f6672f89d9f5 |
| Assets/Scripts/Editor/ModalTester.cs | bf3b20ffa69384692c1a84777e2f2cdb8db47cb001803cbcb311927b5b1759ad |
| Assets/Scripts/Editor/ModalTester.cs.meta | ed19693062543897f0e02ed2090f5c6b78218c37fa2fd28fd7796cab210cff5c |
| Assets/Scripts/Editor/PauseMenuBuilderTool.cs | 528db6a7befcff00905427e7f39853a72f4b72763d752b4906638b5908688c50 |
| Assets/Scripts/Editor/PauseMenuBuilderTool.cs.meta | e582ad4f759590963a4a6f32ad43b3e8026048dbf444426a87032c4e43bb65a7 |
| Assets/Scripts/Editor/Phase11PlayModeTestRunner.cs | ab25b6dd31e82839d0a85ebf400fa0943c782044fc5e0aca74d9f2a744e3fc57 |
| Assets/Scripts/Editor/Phase11PlayModeTestRunner.cs.meta | 094bbb38887dafa88ee8ea0d9505fe0e5b56bb9c527ad8c7b45c11be863b2e14 |
| Assets/Scripts/Editor/Phase11SceneFixerTool.cs | 69fdddf24f9282edc170b74ea910f955bed61b580396fe2ff8c787eda9356efb |
| Assets/Scripts/Editor/Phase11SceneFixerTool.cs.meta | 0fbe5a5e3cfc262647fbf5ceddcc9f6e182cee45fd95b19814427f32042e0961 |
| Assets/Scripts/Editor/Phase11TextAuditor.cs | 404e5d9eab1f5a3d3ca0d09bece7d06c77de0423caac26a66d25dc6219e81a03 |
| Assets/Scripts/Editor/Phase11TextAuditor.cs.meta | b0e6725e747f603bf7175e9afd705cf76b7db34d036ca5e97e3f5a4d7f2c32fd |
| Assets/Scripts/Editor/Phase11UIInspector.cs | dfe41dc4c56b0f63c7311fb3dcc734b49e9cc616b74e2eed6b21e8e6ebbac36c |
| Assets/Scripts/Editor/Phase11UIInspector.cs.meta | 8b7b12f5cce5238ea9e396ece74212078567528a72646103e956f3ee8705a15a |
| Assets/Scripts/Editor/PlayModeUISaverTool.cs | 1f832f7898dd7201945de32995dd883b1ae7279dc52b41334b1976d1906c28da |
| Assets/Scripts/Editor/PlayModeUISaverTool.cs.meta | 9343ae634b39f4d46d1d9190ded711f0f0ec45532f71a263da43864422966ed8 |
| Assets/Scripts/Editor/PortraitHudPrototypeBuilder.cs | 440779f01019d740148c7a7a50da80984c3924c2d4cafdbfa3e9d8b27a459436 |
| Assets/Scripts/Editor/PortraitHudPrototypeBuilder.cs.meta | 243b45416a2ffd1dd2dcd2ecfa3540ff7e039632f60bd6f82666ae61793eed26 |
| Assets/Scripts/Editor/PortraitHudPrototypeChecks.cs | b6708ceedd367e8340a9887ded93cf8ce9d3ca8accbf67cfcd9cf2753d75c976 |
| Assets/Scripts/Editor/PortraitHudPrototypeChecks.cs.meta | f55967069e7526d8d13b6b4c1682be076547b7db087deb8b5668b73438ba4195 |
| Assets/Scripts/Editor/PrebakedSceneUITestRunner.cs | 6c01ca6870953f10302d326bc1bb5b713ef9ae650e58eecda4f389a513fcba9a |
| Assets/Scripts/Editor/PrebakedSceneUITestRunner.cs.meta | 1d3699a6eebc97fba107eede7c94e8c1f3931ccf7e7c3c6e2dc17d6a7c069f65 |
| Assets/Scripts/Editor/SceneUIAutoFixer.cs | 397a6dd114ae9fc55a5243c1b1999fd5dbf84204af7d8cfde6adbd86f4413e49 |
| Assets/Scripts/Editor/SceneUIAutoFixer.cs.meta | d41272ac002290875a3aa454d561e8094ceb42d5eb3698aab5e8b109d66ba1d9 |
| Assets/Scripts/Editor/SceneUIPrebakeTool.cs | ad2bdf30721b80806ba2ec3e8981590d86e9755c8bcd52c66f3dcef6fbc98901 |
| Assets/Scripts/Editor/SceneUIPrebakeTool.cs.meta | 904b7f2890262b80296748a711b6b27e65f2629d50babed374abe5cc16306f47 |
| Assets/Scripts/Editor/SecretRealmAssetSetupTool.cs | ae355329405c7472d55c42800fdf9c9d2ab7f5ff58aac2d0b145b4fe26f64473 |
| Assets/Scripts/Editor/SecretRealmAssetSetupTool.cs.meta | b0c16fb1f47dbd3834fbc98de826b18f24431c1e3093cc996be499c1a84d9712 |
| Assets/Scripts/Editor/SecretRealmTestRunner.cs | 209ce4f39f9091bb748f7eb6218d87072a91c84b6f327991e92ce6c38086d0ef |
| Assets/Scripts/Editor/SecretRealmTestRunner.cs.meta | 6b468c3891273f7679236934e3a4c35e1334574541b6a866d9c2273e73053c42 |
| Assets/Scripts/Editor/SkillBuilderTool.cs | 5a5f50c4f40c0bd74905df5936531e77b316a545e3e0f77f5eee1f4b3e276bdf |
| Assets/Scripts/Editor/SkillBuilderTool.cs.meta | ed4cd81dbd878158d739258f11074cf8521bd198328ad766ea5b0cd9a28466a4 |
| Assets/Scripts/Editor/SkillVFXTestRunner.cs | 0d9fb3a6f1870317840be4b7e18170440a712a71914ba071e4899f0018cb4874 |
| Assets/Scripts/Editor/SkillVFXTestRunner.cs.meta | e894a7328e2092bf49ab673b47568e4585059aad1bd59b5faac907ab7624df26 |
| Assets/Scripts/Editor/StageInspector.cs | 75c40a203e12ae74132c45a8b4a2ee29783cd85211112014393e243e86c31b02 |
| Assets/Scripts/Editor/StageInspector.cs.meta | 6e91c02706da0851e1c42caa3a8c34b05cb16dcca61cb0537eb807f3cf9424f9 |
| Assets/Scripts/Editor/StageUITool.cs | c8afe400d57e5080134e5d9859343f5be7a0b4a1dd557bf071d9401fcf11e71e |
| Assets/Scripts/Editor/StageUITool.cs.meta | b6c67344da3d4f493601eacacd107ce3e26f1cb08784073e3b290731a3081651 |
| Assets/Scripts/Editor/StorageDismantleRegression.cs | 86f4956a2bbac8c408cf73465cc27de21a879cd41cae595efc889fa2d40a4cf9 |
| Assets/Scripts/Editor/StorageDismantleRegression.cs.meta | 6580b3f5e1b785c3fa677180ca8bf626f6bc77e8dfd8ed7dd9512f32fe734060 |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs | f923a8aed6df5a5930fee35a5d94895fca1647cba371f3dc6ef6235f1c997e5e |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs.meta | 1b8cce6547c863bdebe9a247972fe95788a133ea8fc0e2729baf8c92e26fca20 |
| Assets/Scripts/Editor/TopBarInspector.cs | c02fe1ba36b2e7e706d8730083797017481074cee06718f6de7e582a72d82cab |
| Assets/Scripts/Editor/TopBarInspector.cs.meta | 42371c907df3d571145dc5eb40a13b12fca6478bf688e7873ef30a785f80bc4b |
| Assets/Scripts/Editor/TopIconsFinder.cs | b7431641ef60e69cf8f472b59e609e8f2bc56f5d7d1ebf7d0defda4897615111 |
| Assets/Scripts/Editor/TopIconsFinder.cs.meta | 14e42c5df5ad83347cdd0dd07ad54138ca0c592011e7db71466722230e3bbb9b |
| Assets/Scripts/Editor/TribulationUIBuilderTool.cs | efef3ac5419462600f314b5e2570ce214751311f42645651d5e3b836df9aeaa8 |
| Assets/Scripts/Editor/TribulationUIBuilderTool.cs.meta | 305f20cdf09b44e1b7b282cecf1d8eeae0cea3e24ab1e68557c8fe8830dc869a |
| Assets/Scripts/Editor/TuTienCheatTool.cs | 9af259ab939cb84704a17be981967910d1f0372b92b3c0f5d2fbda57c6a03434 |
| Assets/Scripts/Editor/TuTienCheatTool.cs.meta | ee4a1f4dec80cfaa0cd4d871179f2281fc184ed544bf782aea760ecf2eecef45 |
| Assets/Scripts/Editor/TuTienCloudBuilderTool.cs | 22d3c296d18685906b5d7e8311a24e4f2ca19965eb171849af6075432a5b8dad |
| Assets/Scripts/Editor/TuTienCloudBuilderTool.cs.meta | d5fd5558657d7b70039d5f84c50c1433c786e581b3dd1fd7c989a617728e95db |
| Assets/Scripts/Editor/TuViUnlockProgressionTest.cs | f3f1ce71793d659cbf3fc71e9d3db0e1a1feed0cb1426d0c72f487897f5b1360 |
| Assets/Scripts/Editor/TuViUnlockProgressionTest.cs.meta | 0232fbe42d2e8fe89558f2eddbcf6cd7e288f6ae64423315e5d2a73ba6977fa2 |
| Assets/Scripts/Editor/TutorialBuilderTool.cs | 377b0334c50b3294b05cb16f3ce2fd34011b163e8ccab59341356cc211ac7918 |
| Assets/Scripts/Editor/TutorialBuilderTool.cs.meta | 081d1151c2da59cb6688642f2ce6eeb80b7a67e3eb35f315c5e2efb822c9eab1 |
| Assets/Scripts/Editor/TutorialManagerEditor.cs | 047c646a0e0a88d8ca78347649e9554e2cac82ff4eb2cb640a92d9dfb71c6cca |
| Assets/Scripts/Editor/TutorialManagerEditor.cs.meta | caea1985076b723ae1a6a183d7504e4fe303a0b4e5089fb8f98112dad9d033f6 |
| Assets/Scripts/Editor/TwoHourEnduranceTestRunner.cs | 3e97fde5da2cedcb7c9f4ffc12153b2510ea2d221e536750d79d961052d708c7 |
| Assets/Scripts/Editor/TwoHourEnduranceTestRunner.cs.meta | 85bb31b42fa03c9b3333df681fac1107b643a805fbd4d9eb91b7706c64d5214f |
| Assets/Scripts/Editor/VFXAutomationAllPrompts.cs | 3fad9cfd220d93958e13faf7d6557a81406a3ecf326c88fba87429ccb126a7f5 |
| Assets/Scripts/Editor/VFXAutomationAllPrompts.cs.meta | 5d1a2221a98de9e011bdebb1194cbee309286da588972154cf95bbdbbfb8a4ea |
| Assets/Scripts/Editor/VFXInspectorDiagnostic.cs | f78de4d5c075e8a3be314e2b17a2a3d9d79c85b5ad8c5719887c3f5ce9c29cc1 |
| Assets/Scripts/Editor/VFXInspectorDiagnostic.cs.meta | c14320cac314d81485b69d3f698649364ff760ddab4b4e550b549e64a8753d1a |
| Assets/Scripts/Editor/VFXMaterialAndModelSetupTool.cs | 583ebfe49558390c82caa8cbf6f994b64b2ea67e09fb0c507d7beafd4b6873da |
| Assets/Scripts/Editor/VFXMaterialAndModelSetupTool.cs.meta | ed766b9ff997eecbc275e93801cfdb8a9677fe9ee68f4196f91d2cb1edf20d3c |
| Assets/Scripts/Editor/VerifyPauseSliders.cs | 92be201df2e57d3cbc46cc1c9209701ae4af936850f42bc5d72e85b27c41faad |
| Assets/Scripts/Editor/VerifyPauseSliders.cs.meta | 4e6dd83cea3e0b6219d2e132f7c2f179a487548c3329138b3105d5b5d90e6d41 |
| Assets/Scripts/Managers.meta | 0808ae5ae3066602c27b64e06c27f0bef3f20c82fe5dee97a7ee64753b2471d9 |
| Assets/Scripts/Managers/AchievementManager.cs | 7dd5159a4cc3042c7e5720c38c13529199a718ce43bba6d6c0152457bfed6ef2 |
| Assets/Scripts/Managers/AchievementManager.cs.meta | 378bceb1a1234a1d4bb5d20cba8958f8f8d76df979ced70658f103f1e76e89b7 |
| Assets/Scripts/Managers/AlchemyService.cs | b7cea150c39b973f427fbacb3e23dca520c84f2e51aff773c766b9e6d03bac04 |
| Assets/Scripts/Managers/AlchemyService.cs.meta | e606e5ba68dd071f12d4fe339eaa8b6d25a11a9ac4b2ef2e1c2a92febe85a874 |
| Assets/Scripts/Managers/ArtifactService.cs | fe12723a331ee20c317f676bb724e5a486ab2fd2bf1d797818e35351a2475020 |
| Assets/Scripts/Managers/ArtifactService.cs.meta | 4253625fc4fda9b136eeaf8a15b0aeaf35d4bf9ba0a5b5d92b41c0926389ea39 |
| Assets/Scripts/Managers/AudioManager.cs | e16777ab470280456323e438befc2a4b523ee536a67e6c00291b0653248401e4 |
| Assets/Scripts/Managers/AudioManager.cs.meta | efc4ec3be7875076943d784e9582b60bb150e904ca5eadec17fca03c4350f099 |
| Assets/Scripts/Managers/CameraShake.cs | 581affa924204e521990a73e97dedc9fefd42b00929621e4945dcaa093588996 |
| Assets/Scripts/Managers/CameraShake.cs.meta | 5e86fe192eac9a596ba270bba193d0aec3e4f3a52e38bfc93c2d441e20e1aa71 |
| Assets/Scripts/Managers/CombatManager.cs | 10ae0c1afedcdca550086acd34f8da9aec0b2c9c568c3c4d3d50b97d5e02fa9f |
| Assets/Scripts/Managers/CombatManager.cs.meta | 17004d8c71bdc3ff6b0db113690785e0ac179c62588720909566d9cbf8366a35 |
| Assets/Scripts/Managers/CultivationManager.cs | b96a063f25936e1618568fa8cbf7bfacd6ef9c2b73d21321e26d4bf9889329fc |
| Assets/Scripts/Managers/CultivationManager.cs.meta | 3aa9e6b071e2c72578115414aacbaa34439b26042c29f30c47ee488c6607fb7f |
| Assets/Scripts/Managers/DaoAnProgressionService.cs | 060383a1f034b5dabec182ea4825bb194c01c2e0f36588f9e514460ceef4c093 |
| Assets/Scripts/Managers/DaoAnProgressionService.cs.meta | 5eb6810109ee7cdf4125252b6360f8eb4ce9cb10a3fcefffd304a7af46d68114 |
| Assets/Scripts/Managers/DismantleService.cs | a6439ed4ab2ce29e9881398d06121f723945e61c6b87bae75e9e029d1da016bf |
| Assets/Scripts/Managers/DismantleService.cs.meta | d81c288399791955335920c0f271b9094a5f9536a0acfe38791c15c6a56b4eec |
| Assets/Scripts/Managers/EconomyManager.cs | d08fc9babb871d4b91be02d33202ac8920e08bde95ed3124838924bf79c7b275 |
| Assets/Scripts/Managers/EconomyManager.cs.meta | 0c258c2b90896a957e6534290c535b2ee30de6d5b60325526ae7b9c45f398c00 |
| Assets/Scripts/Managers/EncounterManager.cs | 4550ac030fc6c2d473c8ab6652ab4094b88a85539b9a57c2c9da9eb92ae7c29e |
| Assets/Scripts/Managers/EncounterManager.cs.meta | 3a5e765ca864aa7047b50778ac29c9708bb1bba1a5046f4e7e7962c25a0a56bd |
| Assets/Scripts/Managers/FeatureUnlockService.cs | c452f2c0dd6fcded90392e6ff53bf665c0de09a88e703669c9d0de03f39dc763 |
| Assets/Scripts/Managers/FeatureUnlockService.cs.meta | 246d2c18fddde6e0246d218e1d7333994f03418a3b5ce700ab046d4684c3ebcc |
| Assets/Scripts/Managers/FormationService.cs | 3a17695bba1d5d7bc4a2dbb0329b41dc0a22e9b0ac8647a61a924e452c26e8d5 |
| Assets/Scripts/Managers/FormationService.cs.meta | 8965c8c42d25d235438d072c6dd5f814bf7680a602c566c48f59b0ef7d67fb90 |
| Assets/Scripts/Managers/GameLogger.cs | 4d9f5f48ec31fea9650f8c9d8344323cad4838e76ca5ba00369e4f4f2ac3f143 |
| Assets/Scripts/Managers/GameLogger.cs.meta | 2517879c8243f8bc9c06538fcdf864db690cdfa345dc06e1e49c1418baa19bd4 |
| Assets/Scripts/Managers/GameManager.cs | 8020553a76b6990deb3601bc85efa09982c3e62f7471d38b6d7530305ed43c44 |
| Assets/Scripts/Managers/GameManager.cs.meta | 126d7ceb29afc0475e00c8fa82de54323b7e7316ce682532ef334a66ee519f6b |
| Assets/Scripts/Managers/GameplayChoicesService.cs | 3ef0b8627d28d431d24487c02d9643a5ac959624013bc516f25f0681dcdddeb1 |
| Assets/Scripts/Managers/GameplayChoicesService.cs.meta | 10107b20744089b13cf5ef8bbf6d8cdbcb8f2e084d7ce6353e64ed83c54dd898 |
| Assets/Scripts/Managers/LootRewardService.cs | cc9e36816cfc96786ca355c751e81c283232232f3dc3adb5eddd1960fc868da0 |
| Assets/Scripts/Managers/LootRewardService.cs.meta | 1bd1c7604dd90c8695535491f07b3a61b9fc9e5a70129f154ad88112a0d1e9a1 |
| Assets/Scripts/Managers/MapScroller.cs | 258f63e7d8b6e8e0523cf2efecde912131d652e0d1f4cf13f96faee8a0d1e21a |
| Assets/Scripts/Managers/MapScroller.cs.meta | 7a19b5d5399ba91ef67d8f8f5b58f1853e112c31ae21501ffd442964567c7b97 |
| Assets/Scripts/Managers/MergeManager.cs | 6f7d342f8378fcf09ffc990d08cc5cda10c43f11250112d8e01124fa176e280c |
| Assets/Scripts/Managers/MergeManager.cs.meta | cebf296aec1550b0583ec78d19293a51012ea46573f184df169f814c4c84ec43 |
| Assets/Scripts/Managers/MilestoneManager.cs | 953e622c6344d8ad442386506f789b8d7f57ad6dcd77ad7c08aac098804c1095 |
| Assets/Scripts/Managers/MilestoneManager.cs.meta | 9a31976f364fa7714ef52870a2a71737bf6e83387314507c7072ecd70b1ad981 |
| Assets/Scripts/Managers/OfflineEarningsManager.cs | d62390da67a3927ba84d244c1ce0a234b27007b0656dd06e8705c522e8985910 |
| Assets/Scripts/Managers/OfflineEarningsManager.cs.meta | e2c66569750f70fba4641e8829c8414c8a84335cb46c69b383b9c417f650cfd3 |
| Assets/Scripts/Managers/PlayerStatsService.cs | 998b90758b05ff7e20a00228a06113b0c0e9b94d27f9947d24d69fdc863d82b8 |
| Assets/Scripts/Managers/PlayerStatsService.cs.meta | e953eee7588226176b0a5fcc75df7a1c214774b9b266b694eca070655c7d81c0 |
| Assets/Scripts/Managers/SecretRealmService.cs | 0d810e72372508c1b7fc7ba4b2c0ce1596f1b1e0c940475250dc2a8c7269c913 |
| Assets/Scripts/Managers/SecretRealmService.cs.meta | 4ffdb2c6285c9249ec75fe97408e640e8c1ed2c6c98943af31354d6170ec7e67 |
| Assets/Scripts/Managers/ShopManager.cs | 4c73a6a249f9c03b71da5cac00e607a921f1520a05599c3ae75e9225fa7a02f4 |
| Assets/Scripts/Managers/ShopManager.cs.meta | 416f3f1ca81a03f8b770492b5810e78f1d28744bd4ec2bb0f4eaf6cb2d07c773 |
| Assets/Scripts/Managers/SimplePool.cs | ba2be2ccbc1315975b25ff0d715c448d30ab273e3fde8fc269ac845719593992 |
| Assets/Scripts/Managers/SimplePool.cs.meta | 5536a91ee54691f78137b9a571a89ebc1bdb13d3784b80eb0b44abf8781a5bf7 |
| Assets/Scripts/Managers/SkillManager.cs | ec2bd10c4474d72553a1fa31132a410327325a77748744448a2768764ced3e08 |
| Assets/Scripts/Managers/SkillManager.cs.meta | 7316fe852a9c1d876a33bb862edbbc648e892bb5634211d5f72b7a9392678ec2 |
| Assets/Scripts/Managers/StageManager.cs | ecf07f49dc7a48196ca2d517aee0d3c52578ac9e2f870565c7f6a4b21e24e401 |
| Assets/Scripts/Managers/StageManager.cs.meta | 22c1981100b9793cdfcae59afb7da7ed63a0b45c95fc98f1e00d3f7546c98ff7 |
| Assets/Scripts/Managers/StorageManager.cs | 1d84dab1f1f8a0466610b7a5820944d1495a8bfa9752df52ef84f892b3fa774b |
| Assets/Scripts/Managers/StorageManager.cs.meta | 9e872b6255e451393b33b8f7bc9998ebb8fed2ffa7dda484487e3cc9b9c88547 |
| Assets/Scripts/Managers/SwordLoadoutService.cs | f790f68b1cc6e68733230f606f9f7daf31537660f3b6c73f4e76e76b98301ed0 |
| Assets/Scripts/Managers/SwordLoadoutService.cs.meta | 11fdf0d62779d09eccc40f55a678b8d9bddbd732a1f22f999eb3d3df95899ef1 |
| Assets/Scripts/Managers/TooltipManager.cs | ef6bd41d1171c39c00cc7494998101c3b356a075ce1f88b476883bdb90c0db1e |
| Assets/Scripts/Managers/TooltipManager.cs.meta | a82e036947c1696b8c22cd0ac05207f786b9d23d6250878fd467f9bdbbf43033 |
| Assets/Scripts/Managers/TutorialManager.cs | dfbca2cddcd6b4b8b8abf94c75869e172f3bda71d74d45156938c0c72e6bf2e5 |
| Assets/Scripts/Managers/TutorialManager.cs.meta | 64e8f33928d64ea8cf49ccecd0d3af528b15a42b5135e81d7f31c4084e40d287 |
| Assets/Scripts/Managers/UpgradeManager.cs | 7fe5201bf8f7df99d9f56c196b04e09b79ba90cba7f67d9d66fcabc839afbb2a |
| Assets/Scripts/Managers/UpgradeManager.cs.meta | 0ee90c3417be337a6dfed00de26f2a3e90b6b807c4a0979cad19237a662b71ce |
| Assets/Scripts/UI.meta | 8f63ea2b815b87d634f70dc84b298fb66ec9c4130c067d5c65ecd4d26d50c696 |
| Assets/Scripts/UI/AchievementButtonHandler.cs | 6e8df4532415383143ae94bb59a68fba1443e8b1e28e4a04539a88390a778d4b |
| Assets/Scripts/UI/AchievementButtonHandler.cs.meta | 39ab2d5b7718008bc66773ff1b3158540254935a77e1f3acc3f53417b8118e1d |
| Assets/Scripts/UI/AchievementModalUI.cs | 4d0b959bd5e66a21677fda7a7cbb25a5cecc24ab3aec73f05a7c4ea35acd5909 |
| Assets/Scripts/UI/AchievementModalUI.cs.meta | 4e5b3663930ac02d07ae6cf6424a143d53dc2b95490ae2a1d8c95c19249dbbd2 |
| Assets/Scripts/UI/AlchemyPanelUI.cs | c4c66dfd86a14e02547c76937b52749984b6269be78c321761444dbc6929db65 |
| Assets/Scripts/UI/AlchemyPanelUI.cs.meta | 6c9f1ab5d3276468852a9c8588945abac6486ea4a963dad7cf5dd86943dbda83 |
| Assets/Scripts/UI/ArtifactPanelUI.cs | 9e6bb4f985d1122feed62ab8cea560aab13bee4d6474e6fd03a339359f8cac7b |
| Assets/Scripts/UI/ArtifactPanelUI.cs.meta | 772facdcc2426a7ad5f085ee21840f4b2a1e99fcf3f27da3a2728aa6d647129b |
| Assets/Scripts/UI/BiCanhButtonHandler.cs | efff33ca2faac4d45176cf49104bc9d70c22da606b0e15bd2f3e6ad5c3889611 |
| Assets/Scripts/UI/BiCanhButtonHandler.cs.meta | 4953d586b49ab60758250cb319b7868114824e609a0776e285e63705d4f168f3 |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs | fa26a7cf2f9bd243600f8e23771c59a5304d5befaa0b0f18ee9c4eb00de91b58 |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs.meta | 6136c2a987628f57f0194e2fa1b1f5b560d690b7ecb0e968b4d40a742c7e4742 |
| Assets/Scripts/UI/CharacterDetailModalUI.cs | c36d7e46589e07994d0c75fa675eee0a7287479cdd706505becabb91c470d32f |
| Assets/Scripts/UI/CharacterDetailModalUI.cs.meta | c486ff82222ed465ec07ea13ce00b9a3c36ee5d2028b30db98170ad29230711e |
| Assets/Scripts/UI/CharacterFrameButton.cs | d4ca6ddfc966498ac48edaefc9ab8445588bacd45e0dfe243cdb9abc21fa7023 |
| Assets/Scripts/UI/CharacterFrameButton.cs.meta | e3b524622128dc4c46d516d923758b9e8e958119017918d66849c0a4f5f80d49 |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs | 70fccd2fd94126d33e141ec87cc050baa0c8c9eedde5e25833be8c77f9834be0 |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs.meta | e551e566feb2ad5d49c37a3ef8ba31d3f4a88730567f9be701063c9f5fa578f3 |
| Assets/Scripts/UI/CollapsibleQuestBar.cs | 5a5fc6de9f06195aacd30708101c3a3cd1c566c22093d3c9dadac5aabd5375b8 |
| Assets/Scripts/UI/CollapsibleQuestBar.cs.meta | 812eafc61c22f87c6d590d32b563b50d9f34f132e01e2509d7d79bd195f728e7 |
| Assets/Scripts/UI/CombatNotificationUI.cs | 2356d102beaf5177468441739bd871d217bef2b933bc51cfb1ae1b2904a1967a |
| Assets/Scripts/UI/CombatNotificationUI.cs.meta | b82247ea552f8d38f50db7925145a06df06b44380470f20f7611d5c12298b8ae |
| Assets/Scripts/UI/CultivationPhaseBarUI.cs | 41714b68bfef17f1ac055b25cfc27cfb39a6c01963a1561ebc66a0a366cbd0c1 |
| Assets/Scripts/UI/CultivationPhaseBarUI.cs.meta | 97ed65fe15422d7bb8cfc482c66270c57ba5c03f0716c91f80589b3370738672 |
| Assets/Scripts/UI/CultivationUI.cs | 7f4db8f297c8e73cf13fb2b9ec96204e3eb1b1a78563e8e16f51ad79f69e8ab9 |
| Assets/Scripts/UI/CultivationUI.cs.meta | 7ab7d8fe9b3af0a0d825b6c81aed59d36c273240852dee052d6e38fd389d8de8 |
| Assets/Scripts/UI/DaoAnButtonHandler.cs | 8850a53020f6ac8289f97671c4b3c49d8360074358ad011810a6df85c3d7ed0a |
| Assets/Scripts/UI/DaoAnButtonHandler.cs.meta | 019b2283f5346d99de7741c81a2ab5438db38265aef158c04e52dcabc9d6d279 |
| Assets/Scripts/UI/DaoAnModalUI.cs | dc0cbd1e80559af283227230c94d0f6107da778413bcc401a89c4953c9e06fc7 |
| Assets/Scripts/UI/DaoAnModalUI.cs.meta | 17ddaa6794de477def163c9e52ae437647fd80e0921fac4370a98933a9675ec0 |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs | 763ac90e9e39dfda3129470699272ad1a969dff53247326eb3ab4164b4027d77 |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs.meta | bedacbadd9bbbae7bb91d0898b0fd440d08d6b00e86486a24c86aaf5c747491c |
| Assets/Scripts/UI/FormationPanelUI.cs | 80e996e79188090cbeecdfe2419ea26ea65cb5b7b55fa263ea5b3a8cb78fe4c9 |
| Assets/Scripts/UI/FormationPanelUI.cs.meta | 31cfb2610bc7c0ab71f09d191dfa1b391d9c5c5e75e33ed31101ff3f8a90130e |
| Assets/Scripts/UI/JadeButtonFeedback.cs | 506f4ebda4ff5ea3e1c6f0e0b1b4edfaa7205b28831f1b61f47c134dcbcec1a9 |
| Assets/Scripts/UI/JadeButtonFeedback.cs.meta | 5e4bd2cf2f5295496ebadb9d9b19d7125723321e5fd82e1cbfcea17ff0bb85c6 |
| Assets/Scripts/UI/JadeModalFit.cs | 8898ea1c0d28b1cf6695100aaed5bfc2d3fd1c0f3c47d794e82fa74c8e277a62 |
| Assets/Scripts/UI/JadeModalFit.cs.meta | a8111da9652a326d31217dabb7974d9090b9d07b1318d37fb60f41deb9f0947b |
| Assets/Scripts/UI/JadeModalPresentation.cs | 92ccf1edc43d0878d4b861b3d59dc35081a3e4d064385f1407c6cfcc4ff481ca |
| Assets/Scripts/UI/JadeModalPresentation.cs.meta | 4d62b44d6cf1a1796040e315c169a6fe3d0843c0113d256f37d6ecca4837f771 |
| Assets/Scripts/UI/JadeSecondaryMenu.cs | 1cd0d8c3be668ebeef1817ba32092a4d6ef5b37b1207cd9b3afa3c0e1fd67379 |
| Assets/Scripts/UI/JadeSecondaryMenu.cs.meta | 4021fea72e8acbc20ec1765f0610dd8498c895aeda846130c5ad66bc27f17b8e |
| Assets/Scripts/UI/JadeUIAdaptiveSkin.cs | b222e189b6181d6b5bada1bf4a13e44b48aa4ea2d7fdafd174807c50c647ad37 |
| Assets/Scripts/UI/JadeUIAdaptiveSkin.cs.meta | 9b2af3210c3ff19b7f182984f75e3963ffa1e18447dfb0bf1a7ec7041769fc18 |
| Assets/Scripts/UI/JadeUIRuntimeLayout.cs | ab34557a062e6bad027fc986a1a65560f4403e18524362462e36e1d65e8184fd |
| Assets/Scripts/UI/JadeUIRuntimeLayout.cs.meta | 9001b34a0ee2555bcb5fb8d71edaf3ed0699b5e1a65b649ce68f638e18f17b21 |
| Assets/Scripts/UI/JadeUIStyleScope.cs | 87647f56018a914e515f062db4e2a41ee7ff04b14bed2ab0b454cdc974011c25 |
| Assets/Scripts/UI/JadeUIStyleScope.cs.meta | 4fcabdba324df67e630b8a21363ff75ab98c4ee8cb2443ff93bb1335fad883fd |
| Assets/Scripts/UI/JadeUITheme.cs | 47f7a0c86d811275d27d1c543a6a75e79815121c35f9bd84b957b6dccbf5a202 |
| Assets/Scripts/UI/JadeUITheme.cs.meta | 790fcd71dfb6f6ff57b662c219d7340f2ae72f18f68538f20e69a7142ffea106 |
| Assets/Scripts/UI/MainMenuManager.cs | 6a6bfd1ad3120cdaa2ebe5164f348bf0453d286c2822125ef28f9eff80c0ab4b |
| Assets/Scripts/UI/MainMenuManager.cs.meta | 85fcad356144fd9a361ed033690606d744f1d63a6d3d78845c48e13b79db44ec |
| Assets/Scripts/UI/MilestoneModalUI.cs | e8d7faff7149e88ddabfefc69aa89a461dd9c6ce07daff8bbd75c8b94f805db3 |
| Assets/Scripts/UI/MilestoneModalUI.cs.meta | 33b8f93848f6a6f5a044e8644ebfa4d7adac451939c698d8ec48f8b42e41702f |
| Assets/Scripts/UI/MobileCurrencyText.cs | 1aa458aea9ab10c8b187790926fac6bcf10777db07febb8468f626da5f91075e |
| Assets/Scripts/UI/MobileCurrencyText.cs.meta | b83451d9a0a605bd35674582392817097f60b9ca7741663d9a96c549fa98e25c |
| Assets/Scripts/UI/MobileDrawerSwipeHandle.cs | 408bae57aa99d7739dca92dca60b15619eb717de849143d967b689c519671d4a |
| Assets/Scripts/UI/MobileDrawerSwipeHandle.cs.meta | 32010bf757bcfebde7be557c30de1f9a8f67e728847232be4bdf663cda5252fa |
| Assets/Scripts/UI/MobileDrawerUIController.cs | d642a1fabd9e959321ca2c5d4bc1d9633f1144bec14d4e47e210670f4bd4550a |
| Assets/Scripts/UI/MobileDrawerUIController.cs.meta | b337d637826077868946bdd54eb87d2ff6fc5051f9bacf73649a46298d2d5a7f |
| Assets/Scripts/UI/MobileSkillHUD.cs | dbd86d6c5817a86e44252b1a90effe66034ce0542fbb4bb68c8ad74f0c5e75af |
| Assets/Scripts/UI/MobileSkillHUD.cs.meta | a832c325449a6816ff9504687227baeb0dbf560cd47cb6807879ee44202fa9fd |
| Assets/Scripts/UI/MobileUINavigationManager.cs | f4b9c6b92001f9b0f04db1469e911d3ecb59e070072c0e9c051fd9c16087859e |
| Assets/Scripts/UI/MobileUINavigationManager.cs.meta | a045bbceadad0e79084db1f694c2e92da5a4362de9443c46c8fe4728a5ad2589 |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs | 993a61a89723aedf4f7da4849400f87930388f571427f59c3887074937ba939d |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs.meta | 78c572bfc4b320aedf2d31a377d898fe5984b922bc423a196dd816c5d56d15fd |
| Assets/Scripts/UI/PauseButtonHandler.cs | 8716db7c344829312918363a77a5d0510be2c8618d073365e5147462954a57a3 |
| Assets/Scripts/UI/PauseButtonHandler.cs.meta | 4e4d922afc94ce3f3461166f8d6aedbb1a98d7847eb87b5d543eba016065d0a7 |
| Assets/Scripts/UI/PauseMenuManager.cs | 76bfcae2fd1386fe51a58d220287e6a670ba42348ced4cdb7d2a6f7b48fd7fd1 |
| Assets/Scripts/UI/PauseMenuManager.cs.meta | dc981be29809823974dc4d002257106af7436b0ea40539e97ff6a6b49773222a |
| Assets/Scripts/UI/PortraitHudController.cs | 18ce4099bada728d1e11aba3d76553ffdf6e1e72e97ac8f913b27cf75a83a5fd |
| Assets/Scripts/UI/PortraitHudController.cs.meta | 7c7cd3c1dc17eb4d7d30a31a1ddf05d31c410651054f6ad5749f0d7ec01d516d |
| Assets/Scripts/UI/PortraitHudLogStyleScope.cs | 2461489c3a6ec6bc24aa1a6ddaf66c1ab6a64d4d36fc056340512370457f6078 |
| Assets/Scripts/UI/PortraitHudLogStyleScope.cs.meta | e0f6a5e2f02c6b3153599e2062ee6af01be730c8267220a81063f13a60197850 |
| Assets/Scripts/UI/SafeAreaFitter.cs | 5efc144ed9b0cea71e860405d6095f5ceb1a4081fac37320c506bc91c58506f9 |
| Assets/Scripts/UI/SafeAreaFitter.cs.meta | 621141bfb52d864e333492ff8624e447041c0cf99a5f75ef59439d86e0773881 |
| Assets/Scripts/UI/SaveSlotUI.cs | 8286a0d6c4f33374fa96ff5c0b297c8a22430860c9a290134dfebc389264b8b3 |
| Assets/Scripts/UI/SaveSlotUI.cs.meta | 6254df1fa7be5a0e7e6187049544f48bada358181736ead510d02079c07ad7aa |
| Assets/Scripts/UI/SecretRealmModalUI.cs | 803bb0d606432c8c1867f8f4d1da62b4f6b670296b782668b99dbc96317419f9 |
| Assets/Scripts/UI/SecretRealmModalUI.cs.meta | 1721eee75f9eb34a4262a4a41b3dd9e7e52c856e1c0bcc5e00d0ac74bd64ad37 |
| Assets/Scripts/UI/SecretRealmPlayController.cs | 23a655a114e47151f1f5269b01f3ce505bd87585835dc7d0a859c70c62f2ee66 |
| Assets/Scripts/UI/SecretRealmPlayController.cs.meta | 5e3b978fc0b632cd7ffef60a736069db24929cd66bc2e6e472adad039d20118a |
| Assets/Scripts/UI/ShopItemUI.cs | ef8ede0c79b4e5cb0e16a948de3c61e5863bf77ded3cffeff217ba76174a0e37 |
| Assets/Scripts/UI/ShopItemUI.cs.meta | 7915208921dab9a0d70565b3586c86d88ea3a751202b688038023ca4ad48aa5b |
| Assets/Scripts/UI/ShopToggleUI.cs | 99a3d790b5ba46dcefc2b0ed9a542213b19930db820cbcb177ef723d201ee596 |
| Assets/Scripts/UI/ShopToggleUI.cs.meta | 33e190f8135999091ffc3e306017f941b7e16c260fdb30b6696a1cb262d8de97 |
| Assets/Scripts/UI/SkillCardUI.cs | 6d1fefab9682e79d3a1089f8be7506fafd35868c0bd4c6515ec6134b5d730538 |
| Assets/Scripts/UI/SkillCardUI.cs.meta | a6e1686762d962f7a49816a179a67c10e807251299d0a3b7c48aa7264cf4a4a3 |
| Assets/Scripts/UI/StorageButtonHandler.cs | c8e57b9e7541f42c602df776c158bd9c2c9d490c1af59478f78b9737860d8b0b |
| Assets/Scripts/UI/StorageButtonHandler.cs.meta | 19453b0a1076e437a4c4948f6f67b79eb002a4c89fd19642b137821cc6d67892 |
| Assets/Scripts/UI/StorageModalUI.cs | 8536086070350586d8ae106fee8f8941f026261d66c74b3013662c2dbb3a506e |
| Assets/Scripts/UI/StorageModalUI.cs.meta | ae1c18c7901a9416e39b66f05a272b19ef8da3d77fb64f3ebfa6489abf1df5e4 |
| Assets/Scripts/UI/SwordSlotLockUI.cs | 40cf042bb52b2eb9ca141aa8440a80507b603f6ba00ffb053954c3fe64fca804 |
| Assets/Scripts/UI/SwordSlotLockUI.cs.meta | e226d0eb919847d0ae81143363bdf48a60fbb14dd78176c2781a2f9a98a1700e |
| Assets/Scripts/UI/TribulationModalUI.cs | 4281196f515dcebd98e3b521128da2b36b5f6bfb0538d2b9ce3decf854f2c191 |
| Assets/Scripts/UI/TribulationModalUI.cs.meta | bc8fcb8c81373cc37d8ebede32105bfa896f7f0cd4b376695fa93de0fe83bf8b |
| Assets/Scripts/UI/UiTextStyle.cs | bc102de928d7c7355291a0295b503eabb8c9f9d3d8eaafb7f0e266c72b91677a |
| Assets/Scripts/UI/UiTextStyle.cs.meta | a96c7485b749914d2b1bd5953f1c0423c168913146da952c5a9100eec1d3ed5b |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs | af9633cc7314ca2ab2d3bae8dac8e1af2ffe8e8e16d6a6781fc6bf7a8b18b948 |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs.meta | afdba84e6bae425d02a527b6947442f7e0c59fef16f0994b3efb25f78079e58d |
| Assets/Scripts/UI/ViewModels.meta | 6a446f77b2593c86bd3c843522afb1dbd5d0dd9cb833130d44ac6ae1ee2d16e4 |
| Assets/Scripts/UI/ViewModels/LeaderboardAndAuthVM.cs | 7eae399b18dec76f13acbe336f41f62378d36276337be3e5f3e5c8078061fa80 |
| Assets/Scripts/UI/ViewModels/LeaderboardAndAuthVM.cs.meta | 2679a0bd2bd1a02ab8104771f0a42cfe33d5d4cf0aef6daf4a518dadf38ccffb |
| Assets/Scripts/UI/ViewModels/MockOnlineServiceProvider.cs | 97950f084bddcdf7232f21d153b0792803a253f9401f429acace95db6d119871 |
| Assets/Scripts/UI/ViewModels/MockOnlineServiceProvider.cs.meta | ce2d63d5187a5271ff1ce638ede92cd440a00eed06808c94b970ccfe29447e27 |
| Assets/Scripts/UI/ViewModels/PlayerHeaderAndWalletVM.cs | 3d3a0b672a206054d04ad5586e1a6fe4ef699a65d7e6738a48d1a8592f354ecb |
| Assets/Scripts/UI/ViewModels/PlayerHeaderAndWalletVM.cs.meta | 189ad2850f20caa1d26383bdaf1a5eec3a8b02877897dafcaa4e075af57aae49 |
| Assets/Scripts/UI/ViewModels/SwordAndUpgradeVM.cs | 9417829db17d414ad450543fff434e8bc119fa5ee3ceeaf570dfbc4d66cb1e40 |
| Assets/Scripts/UI/ViewModels/SwordAndUpgradeVM.cs.meta | 9d27302c1f020e967a4d8a14dfe3a440ab943c5facf2e4bf34838cb344ff602f |
| Assets/Settings.meta | 73c8e19843240273facb0af6bdbf430d719e397aded705629594060ce25ce7ad |
| Assets/Settings/Build Profiles.meta | 7cd9a55353c36f78d6a3f33ff705a5de83d4f965c1590df81dea740b45e590be |
| Assets/Settings/Build Profiles/Windows.asset.meta | 64811ab55e54c237630bd81fb221fbf1f55ce3025c6690357c51fe5752ae2506 |
| Assets/Settings/DefaultVolumeProfile.asset.meta | efbcb026cee6b8e32fdf27018c06b0515296a6ccbb43969e792fea270310700a |
| Assets/Settings/Mobile_RPAsset.asset.meta | 53ea46647712c615fd1a3f2e0dddfa8559eeedc649e9a1dbf93ff913de28544c |
| Assets/Settings/Mobile_Renderer.asset.meta | a148ee2b4987b05b8ff7ae5f9bf84916184f77aefddc821dceb551f5a69f8179 |
| Assets/Settings/PC_RPAsset.asset.meta | bb469b9b6f318e9e9bed29c3b2e73e79371c04743c6f95dd6603941f1cd3e53e |
| Assets/Settings/PC_Renderer.asset.meta | b5df8ddb2708c6ed4c133a5ff9ee95a75cc20e4ee3ed6181366a61aedbaf31ec |
| Assets/Settings/SampleSceneProfile.asset.meta | 2e23abc214069f0adbf2b2207f1db54be7876629ec2a3681d0f4aa5b61622ef0 |
| Assets/Settings/UniversalRenderPipelineGlobalSettings.asset.meta | f61661d8bc8dbe531daf09f6211ef978c7ba145c9207c8e76524396ae0722d7c |
| Assets/Sounds.meta | 761ae1ccbafae16c1218a0ecb4e0ed391f8ed77b7882997538332b57e05d2b78 |
| Assets/Sounds/LoiPhat.mp3.meta | 7e0aae1c4a63e5fd3a7428a6d23300caf603430d04b4878d26e1f76a82623c21 |
| Assets/Sounds/NhạcNềnInGame.mp3.meta | 9d9c21d139266ee4f8f5909a2fceeb17e30660393f6d400505dd943f83b0771e |
| Assets/Sounds/bang.mp3.meta | ff7fe6618526b604be3aab14ee48d638017b2f659528d4dec0b0cc700f1ee66c |
| Assets/Sounds/click.mp3.meta | 98b9814cd63a4ffb648995fb45011acf164b81e146fffd71b7c3dc1422a5f0ae |
| Assets/Sounds/combat.mp3.meta | 47a25027f4c29affb3517a63c066bcdefd6881550876cad3c7b7c202428d47aa |
| Assets/Sounds/mergesword.mp3.meta | c7290de6de4b34a0d34fa87d28c9ea9ae1674d60d22f6d39fdd23ba91989b4f7 |
| Assets/Sounds/nhạcmenu.mp3.meta | a665b4c381dd74fe58205e38711071271498f4d73122940362c21f4175ecb31e |
| Assets/Sounds/sword.mp3.meta | 6bbf3afb5a31b5d1e190c829065488220c0e81ba0b0dbc7e8f363ba0616249bc |
| Assets/Temp.meta | 6989f2c828693624fedfd17b3bf3f50efaa9ab6a465fc0760722c731949c3e21 |
| Assets/Temp/PCStandaloneSmoke.log.meta | 6caccd69487052c2ee812b667550f616cfeb0fd25b41e0ea6d694e446f03ede3 |
| Assets/TerrainData_c014ab06-8114-4426-bd3b-48b0835dda86.asset.meta | 3fe17219c6ebef665fa6d20aa46854b20f142bed70e1ebcc6f199b1edf8f0b68 |
| Assets/TutorialInfo.meta | 07f022e5932c833d026e020b27d73452ebd781993b616d76003494af1e140946 |
| Assets/TutorialInfo/Editor.meta | 7036bdbc8af05799b5fd508484343b35c28a68b390198da4673d34357d7d667f |
| Assets/TutorialInfo/Editor/ReadmeEditor.cs | 9cd2e203d3fe77c514424aaa42d6b972d8a921f7dbb621134ded71758617aefb |
| Assets/TutorialInfo/Editor/ReadmeEditor.cs.meta | f59080b424c16fa13edd105bf006fdbad98c11fce0c921043e63c8877f144cd9 |
| Assets/TutorialInfo/Icons.meta | 546e7d7af6b4d5e17d284c8f1188d05472e681911b8359905ee9b9e20769a106 |
| Assets/TutorialInfo/Icons/URP.png.meta | b330f1fcfc4e6ceccc95fc34a38434141603469b135633e1c4e8a03c19f9fdbc |
| Assets/TutorialInfo/Layout.wlt.meta | b8745a251df078ffa75dbaac0c4c6debecb61ab306b5b119a490ce6c635a11d8 |
| Assets/TutorialInfo/Readme.cs | d3afe9c7548461ac517ce4bc3bbed240309af830ad57baf66a3a01954c740f75 |
| Assets/TutorialInfo/Readme.cs.meta | 80a3df9969fab6e656583ee9bbca884d0507e2b7de3417f6ea3118fba901cd60 |
| Assets/TutorialInfo/StyleSheets.meta | 930d0c8276cea1559a3e315f8500ea6ca8b58d980eee3b44aaa1d461742c47e0 |
| Assets/TutorialInfo/StyleSheets/ReadmeEditor.uss.meta | 0b7d3988e424498e61b8ad246996cbdef616dfceeeade3d8ebe974546f13f5ae |
| Assets/TutorialInfo/StyleSheets/ReadmeEditorDark.uss.meta | e42f7f9dda8a158ebe18760e1ad684eb113ef70061a046955db87592610bd8f2 |
| Assets/TutorialInfo/StyleSheets/ReadmeEditorLight.uss.meta | 0e4af29f8addccee1c225bd695b280d2e712e59b8801b574481a934f7f78c2ed |
| Assets/UI.meta | 4917630420b7e399d9a4b841eba073ff3bb6a426d9a76b3f3be0b49facef1c50 |
| Assets/UI/Animation.meta | 321e08c9fd34493233bed4eac38a0fb7f94b904c34b54a56f7d7eed4b7417b8a |
| Assets/UI/Animation/Clips.meta | beeccc7581cfc480322a815226b52ee8d2a96eadf041380764a39cbe004905b6 |
| Assets/UI/Animation/Clips/Preview_ButtonDown.anim.meta | c0850f3d87ed3bd1e9c60bdd06946ab2ebae19611d8c886422026693dae06d5d |
| Assets/UI/Animation/Clips/Preview_ButtonUp.anim.meta | eee8a05174ce1afbcafca5880b184da81dd245f52e1a19cb3633ec26ed3de4ee |
| Assets/UI/Animation/Clips/Preview_ModalClose.anim.meta | 76fdf27e04c745d53cddac4985949b9e9c19e6abae808b0667a344f423cacb42 |
| Assets/UI/Animation/Clips/Preview_ModalOpen.anim.meta | c3a8faadaf41d3d29044f736fe7ec107b64b159ed974de19ca79e3f23ebcff8f |
| Assets/UI/Animation/Clips/Preview_ProgressAccent.anim.meta | d8469eb000d966325659102edf80fb3485553b58f261118d99d64f682d227ca9 |
| Assets/UI/Animation/Clips/Preview_RarityAccent.anim.meta | 751738dbed8d64d4fc4f3a8abdf1f416fa9cb13ae1d5fc9f912d89faf07603fb |
| Assets/UI/Animation/Clips/Preview_RewardReveal.anim.meta | 85693e73ee748eb532c245ab0bd581aa1c9b76b0f2818fcc7c1530cb8037e4b1 |
| Assets/UI/Animation/Clips/Preview_SheetClose.anim.meta | 5724b2c60d41fec5513bb64e6e7873bbc5c7cf97c6d18ceb6260dc33d2db496e |
| Assets/UI/Animation/Clips/Preview_SheetOpen.anim.meta | 2093cfe996c17e479d7f49e84cf4d579f9549713782432054e4ab006a76be750 |
| Assets/UI/Animation/Editor.meta | 19c88f042b52a16725009ba512d3aea080b7cb1e64950e0603f54436f1ed7007 |
| Assets/UI/Animation/Editor/MobileUIAnimationBuilder.cs | 54f785ba945e05461aeac18de4a716415e080197d67d419c8472a17c8f2c3d63 |
| Assets/UI/Animation/Editor/MobileUIAnimationBuilder.cs.meta | 7498e553a130dfb3e79584b1cfb2255231dc157fba5655e19bbc3168b0cef9bd |
| Assets/UI/Animation/Editor/MobileUIAnimationChecks.cs | e6ba581a8e6347e1c1c8246e83c4683d4737f2fdfa4f36fb1827f89e0379dfcd |
| Assets/UI/Animation/Editor/MobileUIAnimationChecks.cs.meta | b3f897132c608877b20cdb458e2bf0c8e3cbfbecf6d8610693bfd86759e4bff9 |
| Assets/UI/Animation/Editor/MobileUIAnimationSimulatorChecks.cs | c64d7243397fa618ee7ff17094e07b1cad7fff5ec6f5707d4d2a7f5a7433cb1d |
| Assets/UI/Animation/Editor/MobileUIAnimationSimulatorChecks.cs.meta | aeb0f40babae22453625b6c989ac274e9bc838204ed6bd5da617e6dd6bb42343 |
| Assets/UI/Animation/Editor/MobileUIAnimationSuite.cs | 1177c3f822735ee8012047a383cf1ce456fda812c3ca0a72f41b6f810f09d542 |
| Assets/UI/Animation/Editor/MobileUIAnimationSuite.cs.meta | 15360c57eae2e57c3ce2192184de6964321efaa668150dcbb3c71b2d6d6b8eff |
| Assets/UI/Animation/Profiles.meta | 047825ead5daf48c1971e0fb831cb5cf7a0f7464eb78bb8f465138409bf46cc0 |
| Assets/UI/Animation/Profiles/ButtonDown.asset.meta | d4c31111b2e9a8bc67f813edaa0598af8465ce198c74ea610561d94cfb083224 |
| Assets/UI/Animation/Profiles/ButtonUp.asset.meta | 7f4ff6a471fadf1244e1b206a20936d257e86cc173b80ff9014eb39a3aebcdc1 |
| Assets/UI/Animation/Profiles/ModalClose.asset.meta | cbaefec287bfe21c9a5ddfe176d75c952525e4a4f153e3748968917801faeb31 |
| Assets/UI/Animation/Profiles/ModalOpen.asset.meta | fd260865505309ba469cb82a0db21ead9fc65dc7ca191bbe581824759a1b4110 |
| Assets/UI/Animation/Profiles/ProgressAccent.asset.meta | 17b1ea9998d578d9341ea63969e5063cb1edb04291d6ca730d4b83e407d117c2 |
| Assets/UI/Animation/Profiles/RarityAccent.asset.meta | dfa76ae22df7e39c0a3d111931cd4faacdef988d73e114c907c868b6a4295c68 |
| Assets/UI/Animation/Profiles/RewardReveal.asset.meta | 63009eb948f751013a354bf9d3fb91b223ee6d980f4d2c313ed90f575f578213 |
| Assets/UI/Animation/Profiles/SheetClose.asset.meta | d018fc571f4104b4fed67ab6e8e585c703be94d0751419d8fc8ba276344877cf |
| Assets/UI/Animation/Profiles/SheetOpen.asset.meta | 1e5bb170939a3d4f4185a9282836548be8ae8eb9e9f198ac2f60b2ab736d19c9 |
| Assets/UI/Animation/Runtime.meta | c3f6d99f94d65a38a945bb996c1788b7415a07790776b4b12ea1db12f777dee0 |
| Assets/UI/Animation/Runtime/MobileUIAccentPulse.cs | 6ee49f607e951f6e021a65922f6cb24f36c1852063f3ed3edbecc39c77c7c5e8 |
| Assets/UI/Animation/Runtime/MobileUIAccentPulse.cs.meta | 0ddb3bccec3cac8a959ad9e224b22a72d19fbdafcaf7182c0fbf7f59aa78f4c0 |
| Assets/UI/Animation/Runtime/MobileUIAnimationDirector.cs | aa9ce5b339dd1b45e9e64313fda023eb9d132411a91d5767f17b38706a9688fe |
| Assets/UI/Animation/Runtime/MobileUIAnimationDirector.cs.meta | f671054ca7de1fa6d9fd422abbeffa2dd8ff258d1ce5485c013e34cb86422848 |
| Assets/UI/Animation/Runtime/MobileUIModalMotion.cs | 53fe7795c988a9e3ce3bee064dea81aa8acd06ea822da1ca4171f900cd8a2439 |
| Assets/UI/Animation/Runtime/MobileUIModalMotion.cs.meta | cca6fbad6cc6da162587f8ca78c33028b14ee8e0d93c88843005e78401a7a720 |
| Assets/UI/Animation/Runtime/MobileUIMotionPreset.cs | 6dd8c4097e37038da60d265ef838cc50c4d5640fbcb00e7faeca6e8d24fb03a3 |
| Assets/UI/Animation/Runtime/MobileUIMotionPreset.cs.meta | 3e7dee65299e0fcb37582dc0fee916f82534f9b07294cb74e0e1ce43878d08d8 |
| Assets/UI/Animation/Runtime/MobileUIVisualScaleMotion.cs | 84547c7df0efb6008cc1924e3f78a200380aa4668cd07fca96e9a03159422f7c |
| Assets/UI/Animation/Runtime/MobileUIVisualScaleMotion.cs.meta | 5095088281474e2a0d0c9f841568f14faea9b7b17bf9235d81393d5c3ff32f30 |
| Assets/UI/MobileHUD.meta | 06d38ac0cf2881afd23f83bc344c6640382a77aeb47e573a18101e18d5a02d9e |
| Assets/UI/MobileHUD/hud_prefabs.meta | 99ceeb56599ae56b3d80075ed0ba698b3e0efbcffd7739a17f6e4865f12c7872 |
| Assets/UI/MobileHUD/hud_prefabs/HudActionButton.prefab | bf4c8d16757f0776ccbe3e1722e321916228dfcaaa2a6243a65290f6c21874b8 |
| Assets/UI/MobileHUD/hud_prefabs/HudActionButton.prefab.meta | ffb89639bb7057066590e025ce5b8ffb27c9000644495aff4c7c03c8d8182929 |
| Assets/UI/MobileHUD/hud_prefabs/HudCoreDock.prefab | ea3a3b4ae921265716ca85e98330d387126fda6f80cc0b49008f0c0a6989663a |
| Assets/UI/MobileHUD/hud_prefabs/HudCoreDock.prefab.meta | a9d1716dcbe2acda54a202192c978c4b60b2d7cdbf9cd93ac2c7931cc89d3fa1 |
| Assets/UI/MobileHUD/hud_prefabs/HudResourceChip.prefab | 32ace335a8f49008519fe4087d8c1796a59f49be43c304992701d7d840fd75fb |
| Assets/UI/MobileHUD/hud_prefabs/HudResourceChip.prefab.meta | b82af03c68f02a4ba0e62f66b4b5a9d9be13b108dea440c1fcff7d02a6f9a04b |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab | d6089ae1e2ab7933db0c86be9aa6190e0b84555eca5f50efcca92f786d8077b3 |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab.meta | d776fb10af5ccd58192df53792414784343db7eca60b5a7c0d041d3bddba39cf |
| Assets/UI/Optimization.meta | 5cce3135166f53fdf1fab33c74f62d39dca0b41686332ab51e4a3f420c432ce8 |
| Assets/UI/Optimization/Editor.meta | f82110a456500638137e01b9d72a7a9318c4b22d3efeb9f96a48aa6ac06f83bf |
| Assets/UI/Optimization/Editor/MobileUIOptimizationAudit.cs | 87e1f79c62669d34cb7f7a429a84697b798485b440ad5c03c3d54f6cc0475887 |
| Assets/UI/Optimization/Editor/MobileUIOptimizationAudit.cs.meta | 38a64a2d595ecfbdd0014a46cc24ee3ab39dee7fcafe3d77d3b576d5256b616b |
| Assets/UI/Optimization/Editor/MobileUIOptimizationChecks.cs | 40410c9ec6b74dcdd531886637c795f16497997d06a2a5fd96ce49c1e522cf07 |
| Assets/UI/Optimization/Editor/MobileUIOptimizationChecks.cs.meta | 29e64c73a168245e0c5713b28da28fb112f6376be5c7c0bd4abf4b0e49896ae2 |
| Assets/UI/QA.meta | 064341ea0ed3e39b72d52b67a6b1183dae62aee03331b452f1c5e19d43e3b343 |
| Assets/UI/QA/Editor.meta | 8c6bec8886fdd924246d5d476fda3a53c6c4fe5f2f3b195f805dad726058e089 |
| Assets/UI/QA/Editor/MobileUIPhase09Checks.cs | f312092246fcd5f9e33eec2dfc212a7d0cadc8171e01a1ee69c389bbece4ba13 |
| Assets/UI/QA/Editor/MobileUIPhase09Checks.cs.meta | e6beadbaa843a28ea1c2ec590a986b29b5d5eb44e74f2efbdf311f80f66a8d90 |
| Assets/UI/QA/Editor/MobileUIPhase09ContentRepair.cs | 8c16de9951d1e336a33295d47d3c814903071631ef4e6d9e800352535d58cc53 |
| Assets/UI/QA/Editor/MobileUIPhase09ContentRepair.cs.meta | 02b633efc408f52ce68244ca657d047c6a089795282fa3ffabfc88fcc32a2753 |
| Assets/UI/QA/Editor/MobileUIPhase09Extended.cs | 932fa178037d7cce2ca3dadad55f90950c46dc5597e993ca014685bf53ac70f5 |
| Assets/UI/QA/Editor/MobileUIPhase09Extended.cs.meta | a8ff1287d76684e2549a163a78079b5a83696ddb0124c60c067a1c90916d3b50 |
| Assets/UI/QA/Editor/MobileUIPhase09Extra.cs | 714b9725373522c6aab94f8e974e1f7d7d32e3f5c8c35ae586fae46e39c60859 |
| Assets/UI/QA/Editor/MobileUIPhase09Extra.cs.meta | 46b69919876f6152c60ec4ee230f2ad11a6954d90fc408824307e7ca307476df |
| Assets/UI/QA/Editor/MobileUIPhase09Regressions.cs | d52dd3bf2b580e2f379793897d70eae8addf99b69bd019d421f70c6dc91d0f11 |
| Assets/UI/QA/Editor/MobileUIPhase09Regressions.cs.meta | 38552feb9281eb8a3530799fc40a13c4301fec84827520eebdf3895d2fecece4 |
| Assets/UI/QA/Editor/MobileUIPhase09Responsive.cs | 42cf1bf81b7a0eb57ec7f8435740b0ea2f1977d8588f566ac296ddb720b06254 |
| Assets/UI/QA/Editor/MobileUIPhase09Responsive.cs.meta | cbfcddb45acdfbd7bb976256d87429716e7fc5819c40f3f7641ae77efa0b49b4 |
| Assets/UI/QA/Editor/MobileUIPhase09Suite.cs | 33b99ec3b1f82a9afa3247d88585dbbe5e7cb811c6f86a10024be98a446c355c |
| Assets/UI/QA/Editor/MobileUIPhase09Suite.cs.meta | 371daf03239bcfa26c0f25328cd136b5b32b8c990c50b0e010fca8cdcd41baa8 |
| Assets/UI/Reconstruction.meta | 94e49f8e24d1c3ee968e81ee83e893590c316cf733f62e9e9fde82774e35b475 |
| Assets/UI/Reconstruction/Editor.meta | 68d6314cc5e8ea8b17e3ffe668c7210701cb8d9504ab183baa6e2082dd900b47 |
| Assets/UI/Reconstruction/Editor/MobileUIContentRepairTool.cs | 2391a656ccf4473b3401d6347b7be042fface1c64ac770200955f89cc3109812 |
| Assets/UI/Reconstruction/Editor/MobileUIContentRepairTool.cs.meta | 16bb8987717be4702283cd163ae7fae3e34e74c7af9af3f64267b28b4020d707 |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionChecks.cs | 784dabb84296059743c5c6be3acf9fbd40da92b9d249bf081d589455e8e1c50b |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionChecks.cs.meta | df6984c0c62e676dbea442cb240522fdb894f8d2b65fee0b2cedfdc4719238e9 |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionExtended.cs | 1f1e99c42877c15137b17cbb8c9add6be33325f5dbdffeef44a0719a0e9a7ab2 |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionExtended.cs.meta | cd791a0090e01f74c4315123c797150ee8f63e84aef48885a73e169d8616bd5f |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionExtra.cs | 8405401bb1f50f43afd1cd31b5e15ac2c92fc97e38d8a4fffcf75e4465965e52 |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionExtra.cs.meta | a82815c5d63f1d0ce3dbdd241f32e3b01163f7d695e3cfd6015df2bfeb97f965 |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionRepair.cs | 147ac9bf0f3b9dc9c1fd9dc7ab706274f55b42511218ab3baecd221745814f8b |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionRepair.cs.meta | 8ebff67058a2c1db0a67177cc9269867de5f83eceb673f9daee522f52357e3ea |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionSuite.cs | 4b50d732ad5251b4e32dc1fdab866120b51c8ac7785ccc5519ca6febafe2b298 |
| Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnectionSuite.cs.meta | 33516601d01b5245b8f7fa5fc7f1a1b1fd9bf7f7e482bf6c6636946ac56a5914 |
| Assets/UI/Reconstruction/Editor/MobileUIReconstructionBuilder.cs | 54154f8fc3824b0d1edd178357e7efbbb176a0338e9f418d341016f7adb08730 |
| Assets/UI/Reconstruction/Editor/MobileUIReconstructionBuilder.cs.meta | 68770fd22541ef803f4c42b29f0051984b9fc9beb74ec12fe7071d59a181bd2a |
| Assets/UI/Reconstruction/Editor/MobileUIReconstructionChecks.cs | 64ab49a9574e692a0566d2dc90acd275f8cae0043325346b542a0b742e0ff167 |
| Assets/UI/Reconstruction/Editor/MobileUIReconstructionChecks.cs.meta | 57b6f3506f762312e20c7b4f21239ac7d0ed81be42691b340c2216e25afbbdec |
| Assets/UI/Reconstruction/MobileUIAssets.asset.meta | 017789575750d4fd797be482637f90ffa37acd326f83b9f69d668c3229eea137 |
| Assets/UI/Reconstruction/Prefabs.meta | a7d7ebc510dbb144049ed615f9ea19f7e3dc3103ec1a0bb0113d62fae9fc90cb |
| Assets/UI/Reconstruction/Prefabs/CostActionButton.prefab | 292b8156c9c9a31a1514a4da2486d284e5d8d30526ff548b02206d07ec815409 |
| Assets/UI/Reconstruction/Prefabs/CostActionButton.prefab.meta | d11a1edf10c275bef2265ddb8b22a3a4c5b1755a97a2fc249f3bd468cece9af0 |
| Assets/UI/Reconstruction/Prefabs/EquipmentItem.prefab | 3286c249a7ff2b4b21e916f679d02edf63045ec2663946b983ab3ea8c0342a4b |
| Assets/UI/Reconstruction/Prefabs/EquipmentItem.prefab.meta | 37ac2303c2e0a2dac3c1ae684e1d1c7ff43932b71e2cb3d87d768ea526e57f61 |
| Assets/UI/Reconstruction/Prefabs/ModalFrame.prefab | 38c398d557177772778733fac053e0a2bd686ce11fb496c58eb99f1bd67f4d97 |
| Assets/UI/Reconstruction/Prefabs/ModalFrame.prefab.meta | ec58819269942a3847aae144291addae8f2d16c0a566da604851cb16ca1fffad |
| Assets/UI/Reconstruction/Prefabs/NavigationButton.prefab | 49f16d8246137a3bfe7e1abe540a079ab80639f0c49cbf6ef452c28fb646b8ad |
| Assets/UI/Reconstruction/Prefabs/NavigationButton.prefab.meta | 4916a45df00e8604a6d062b921cd18fbb02adff9bfa040377219bbd0fdeace09 |
| Assets/UI/Reconstruction/Prefabs/PageFrame.prefab | 84df7fca909aad5a62d6e3933d8f49e9eb122c18cd24338c480d27dfa4e5d834 |
| Assets/UI/Reconstruction/Prefabs/PageFrame.prefab.meta | cdc15707731c8c2167bbfea26fdc1e1708b265bcc3a4e9a1ccaff50c87afcc80 |
| Assets/UI/Reconstruction/Prefabs/ResourceCounter.prefab | 97132b28afa9d80bae2a0cfc38b25ee0774372ba38212dff2247e6f264c52095 |
| Assets/UI/Reconstruction/Prefabs/ResourceCounter.prefab.meta | 721491666dccd90da71f19d12d3366a607d9ce36042c715af8ec8ea4ee29cfbb |
| Assets/UI/Reconstruction/Prefabs/RewardFrame.prefab | deef13da3c045253431742f84ba410efbb8d601273826bd1edeac907523f6239 |
| Assets/UI/Reconstruction/Prefabs/RewardFrame.prefab.meta | 070a2729a61834ec00d456d10ff00de435783ab9976a0413833d2eb33a0967ee |
| Assets/UI/Reconstruction/Prefabs/ScrollBody.prefab | 07644f13dfaad8447968dc88f1eb7ea37289ce836e15371f647a4494b74f7358 |
| Assets/UI/Reconstruction/Prefabs/ScrollBody.prefab.meta | 6909d8266f59153900e26ad0d542f622ce5addd8d3c9c4c0d00ba8ed6c1ced1a |
| Assets/UI/Reconstruction/Prefabs/TabButton.prefab | 78fb8e71cb72bbada4a1ae2eca93a79d518a0b27fd1cd342089d39eb18308574 |
| Assets/UI/Reconstruction/Prefabs/TabButton.prefab.meta | a7e06c4726a6060d20fe8f494f6a350c67686b26052c92090e2af9ba72588b1b |
| Assets/UI/Reconstruction/Prefabs/WorkbenchFrame.prefab | 84c396f319996c04a915dce1f994cbf776848a2b655d4076f344d221c4d70412 |
| Assets/UI/Reconstruction/Prefabs/WorkbenchFrame.prefab.meta | 337309ce183c82a0cbf15b9039cc83f3227b1e4e12e38c3abe37dc2b9743429e |
| Assets/UI/Reconstruction/Runtime.meta | 630c0ff221ba60fe45b5dbc189cdf15bcc1551fa9a469b30889bcbac7b598f4b |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs | b3c3450317d7dd6007db89a4df386a6324391c3db8c776fa6e48b3573dcdcc85 |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs.meta | cdbe6214a65ab1fa748772da4d0f5ae8b692cffa54b79ee788215f188379cec0 |
| Assets/UI/Reconstruction/Runtime/MobileUIAssetLibrary.cs | 0dc9e84219864318fd0436a7ab771061907447c02bf3d9aecfa99f514a5059d8 |
| Assets/UI/Reconstruction/Runtime/MobileUIAssetLibrary.cs.meta | 669f41af089b18e7e754c3b1663f1105ae15d6a7e22adb6210e8d00b94f66c4c |
| Assets/UI/Reconstruction/Runtime/MobileUIContentSafety.cs | 7e40964c3b836df3e4f8205cd245bb9d0fba7e361ba5d7631e6198add7ac005b |
| Assets/UI/Reconstruction/Runtime/MobileUIContentSafety.cs.meta | f66786cd7281df579b562b0ecd8f10d1a26b65e7f8dda43d61dc4b349c8af2e1 |
| Assets/UI/Reconstruction/Runtime/MobileUIItemView.cs | 5f5415cb1d0d091def906b792a6f180c958e020ab2346a56557314838172580e |
| Assets/UI/Reconstruction/Runtime/MobileUIItemView.cs.meta | 7005326317df8f4a12dac974399b7aae2fdd67a7715749172e485705fb52ed52 |
| Assets/UI/Reconstruction/Runtime/MobileUILogicalViewport.cs | b450a1e6a1612e6924bf60b16d0b270442496632f1f4748e86c0adacf367e8db |
| Assets/UI/Reconstruction/Runtime/MobileUILogicalViewport.cs.meta | 867961cacc346528b7865de67bfa3c40ca2565829a4ba9f6bd71306b702ae40a |
| Assets/UI/Reconstruction/Runtime/MobileUIPanelView.cs | 7d8cddb937e5a5b6f7544eb71cfbcc90e5dfc419f90982a01bb24f5105d6491d |
| Assets/UI/Reconstruction/Runtime/MobileUIPanelView.cs.meta | f80d64cc1bf4c7ac8f09824cd94082336b2e552c1c0a2d0c66f9e598d32df143 |
| Assets/UI/Reconstruction/Runtime/MobileUIReconstruction.cs | 2fe8df9dfd48476f2801e8366598b9da01abb78e1ceb5cba30f44fd63014dcdb |
| Assets/UI/Reconstruction/Runtime/MobileUIReconstruction.cs.meta | 90840741a66d7f1c122e5061463f445ea7c6c77a79e6c4321670459d7c1a4828 |
| Assets/UI/Reconstruction/Runtime/MobileUIResponsiveRepairs.cs | 404ec1d4c42847cc3278513c20bf8f3a906e48b74a735be8ee6053293d201040 |
| Assets/UI/Reconstruction/Runtime/MobileUIResponsiveRepairs.cs.meta | f65e119c3a87ac80ea54756825074ab4fa3bd256efffc1024fa656f7ff04634e |
| Assets/UI/Reconstruction/Runtime/MobileUISlicedSurface.cs | 7b6b92456634da85f2b9396896af057c659e10dd56ac3b50b3fe93c1f94a0ab4 |
| Assets/UI/Reconstruction/Runtime/MobileUISlicedSurface.cs.meta | 9da402416c2c78d36ca7f4d3e7f45b7fea17774e5b8f664f67dedfa684120238 |
| Assets/UI/Reconstruction/Runtime/MobileUITextDensity.cs | cc08c9b691111f6fe62d6aa9448a8dbd082c4e1d7b175a020592f9857f0bfe12 |
| Assets/UI/Reconstruction/Runtime/MobileUITextDensity.cs.meta | 182d764832d9486efaf926a3fbfa1030640f41847cae7a047619c6fc8a03158c |
| Assets/UI/Reconstruction/Runtime/MobileUITutorialOcclusion.cs | b3e94a244dcd75fd8f22f58952aabbd37caa902ae55098df9fda305645881e91 |
| Assets/UI/Reconstruction/Runtime/MobileUITutorialOcclusion.cs.meta | 47bc9f67f5e3ae904b883b6aeae99dd0ad09fbb6af7542cf912e8ebd7d05893c |
| Assets/_Recovery.meta | 35147fcf4e1d3ba767dc2786057fc14ed73e52d1c599b453a6e5ae5a0e172075 |
| Assets/_Recovery/0.unity | 519fff921de3fa12f90d323fb0d406eee0b87f56685508f2ee47804538c1c213 |
| Assets/_Recovery/0.unity.meta | ffbfc172e492d3b78d0fb1d09145ecf4b5800564631d6e38c00b036e7477ca36 |
| Packages/manifest.json | cf42d1605e911c5a2071fab94147562ad1d60ded74133eaf4b4a94458f5c15a5 |
| Packages/packages-lock.json | d4ccc4c381cecfb99c17116186f033acaa33e8f41dae2db4c6f3a6c7bac8e1da |
| ProjectSettings/AudioManager.asset | aeedeb3243f07b61dbbe854aa08b5ef6f0b5274dd9852ad9f6129dde3e105a31 |
| ProjectSettings/ClusterInputManager.asset | 40e65dd214bf254955d2c8829ca8ce2130fd3510485560ce012e9a1ddd93e623 |
| ProjectSettings/DynamicsManager.asset | bf017b547bb77ce3443dd5287ddf2516a74fd167f4f2ee5d798340694f51776d |
| ProjectSettings/EditorBuildSettings.asset | 06273de58aa38e4aebe9b82f9843f37d74e690cb4d86d0bd75aee30b2480ec52 |
| ProjectSettings/EditorSettings.asset | bbedcf93c32bca12eaba9ed61189ab13f064217e9f2984a66772719fa73c56b5 |
| ProjectSettings/GraphicsSettings.asset | 1aabaa3226af3fdfdcd1bf936ff3ebf164870bf04b2fe230138634985ca62b1c |
| ProjectSettings/InputManager.asset | 7a1bb836a1008d26d0860d143e9d9df7ca41731d30a72901b518063819c62df2 |
| ProjectSettings/MemorySettings.asset | 1e8379a461b10ae09f3f696911088c16875d63e87ed5f2d95df9e1892703f9fd |
| ProjectSettings/MultiplayerManager.asset | aba89fd1a1ddad182727ad82727e536d27372e90328a8196da2a4560398983dd |
| ProjectSettings/NavMeshAreas.asset | 80106cfa7e6181a1928b142c075f3e510d527441e3d79bc83590e4ac91e7cd66 |
| ProjectSettings/PackageManagerSettings.asset | f281d96e785df430d30ce0a515f4fe5c625ef44467125879f4b9f4cb087e1578 |
| ProjectSettings/Physics2DSettings.asset | d57a0fe55659c33879e8b70bc01c5ea9cdf082069dab649905c6c5138531e6eb |
| ProjectSettings/PhysicsCoreProjectSettings2D.asset | bedc5339a832c60dc3f11b84abc4f4384dbfa20b0cd9ad6c444b907e751fc35e |
| ProjectSettings/PresetManager.asset | 29c125923fe6e29f90190d4fce1c82d660e7051546e2c9d02908d18c9cf2436c |
| ProjectSettings/ProjectAuditorSettings.asset | 9e68444b9bda981e2927a98df9edf69ea88362f0c6b7f3c81b39b9d515d2a416 |
| ProjectSettings/ProjectSettings.asset | c33f9acebc8e5e2bbe6d9a179171065819eea1ce339c80bf2ad40c2256313edf |
| ProjectSettings/ProjectVersion.txt | a35c2c87870781882617d3e5fa7188e6e5528821fb74c74e29a43c4559f57390 |
| ProjectSettings/QualitySettings.asset | 600bee1dd55cc67dbdfdcf320395635175c0e762760a8504cceaa080a18a4eba |
| ProjectSettings/SceneTemplateSettings.json | e0cbb98bd0aa9449840ce663a4b81751ab0828f5d8d2bd37923644fee7095e62 |
| ProjectSettings/ShaderGraphSettings.asset | cdba1e5d65bcf6e3269d76ed3e60b6c21f98322002996642c9d74f1eb89f6b91 |
| ProjectSettings/TagManager.asset | 23fc56b484aa3d7edf5914cd3591f7a823ccef5e3f3496f8a51f30388ead63dc |
| ProjectSettings/TimeManager.asset | 1a83e54adbbda7c9f4851103a0c6ab7f6448a3343d4ea5b7620452fa08416ecd |
| ProjectSettings/URPProjectSettings.asset | c317fe22b9488785c0611f0642429922e5d324430d929bffb41648130f30c218 |
| ProjectSettings/UnityConnectSettings.asset | 7f2112b8c6ea0885dcc72b312c9726ffcd909f2e89d1eead61889274a4e5d1c0 |
| ProjectSettings/VFXManager.asset | f066c6483919961332415ee64204f74fe52f38c02e82892eb02b7a5b2e381f0e |
| ProjectSettings/VersionControlSettings.asset | e7bda45bf7a394745fee1316e6c71fa676004d591ad1f3aa247fd3e4ceb86071 |
| ProjectSettings/XRSettings.asset | edd2beb4a2e388312b4b7da241aa51070434005d82c59363f1237dd2e567585f |



## 7. Source preservation verification

1398 file đối chiếu sau scan: {"changed": [], "missing": []}. Không thay thế Git diff hoặc kiểm tra save ngoài project.
