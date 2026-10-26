'由于包含加解密等安全信息，本文件中的部分代码已被删除

Friend Module ModSecret

    '标注 PCL 的不同分支，仅用于替换标记
    Public Const VersionBranchMain As String = "OpenSource"
    '在开源版的注册表与常规版的注册表隔离，以防数据冲突
    Public Const RegFolder As String = "PCLDebug"
    '用于微软登录的 ClientId
    '本修改版自有的 Azure 应用（公开客户端，无 secret，Client ID 属公开信息）
    '环境变量 PCL_MS_CLIENT_ID 仍然优先，便于临时切换测试
    Public OAuthClientId As String = If(Environment.GetEnvironmentVariable("PCL_MS_CLIENT_ID"), "504721c5-1f26-4d60-afa4-ae3f4b65be64")
    'CurseForge API Key
    Public CurseForgeAPIKey As String = If(Environment.GetEnvironmentVariable("PCL_CURSEFORGE_API_KEY"), "")
    '用于匿名数据收集的腾讯云日志服务上报 URL，形如 https://{region}.cls.tencentcs.com/track?topic_id={topic_id}
    Public Const ClsBaseUrl As String = ""

#Region "网络鉴权"

    Friend Function SecretCdnSign(UrlWithMark As String) As String
        If Not UrlWithMark.EndsWithF("{CDN}") Then Return UrlWithMark
        Return UrlWithMark.Replace("{CDN}", "").Replace(" ", "%20")
    End Function
    ''' <summary>
    ''' 设置 Headers 的 UA、Referer。
    ''' </summary>
    Friend Sub SecretHeadersSign(Url As String, ByRef Req As HttpRequestMessage, Optional SimulateBrowserHeaders As Boolean = False)
        If Not Req.Headers.UserAgent.Any Then
            If Url.Contains("baidupcs.com") OrElse Url.Contains("baidu.com") Then
                Req.Headers.Add("User-Agent", "LogStatistic")  '#4951
            ElseIf SimulateBrowserHeaders Then
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)} Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36")
            Else
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)}")
            End If
        End If
        If Not SimulateBrowserHeaders Then Req.Headers.Add("Referer", $"http://{VersionCode}.open.pcl2.server/")
        If Url.Contains("api.curseforge.com") OrElse Url.Contains("forgecdn.net") Then Req.Headers.Add("x-api-key", CurseForgeAPIKey)
    End Sub

#End Region

#Region "主题"

    Public Color1 As New MyColor(52, 61, 74)
    Public Color2 As New MyColor(11, 91, 203)
    Public Color3 As New MyColor(19, 112, 243)
    Public Color4 As New MyColor(72, 144, 245)
    Public Color5 As New MyColor(150, 192, 249)
    Public Color6 As New MyColor(213, 230, 253)
    Public Color7 As New MyColor(222, 236, 253)
    Public Color8 As New MyColor(234, 242, 254)
    Public ColorBg0 As New MyColor(150, 192, 249)
    Public ColorBg1 As New MyColor(190, Color7)
    Public ColorGray1 As New MyColor(64, 64, 64)
    Public ColorGray2 As New MyColor(115, 115, 115)
    Public ColorGray3 As New MyColor(140, 140, 140)
    Public ColorGray4 As New MyColor(166, 166, 166)
    Public ColorGray5 As New MyColor(204, 204, 204)
    Public ColorGray6 As New MyColor(235, 235, 235)
    Public ColorGray7 As New MyColor(240, 240, 240)
    Public ColorGray8 As New MyColor(245, 245, 245)
    Public ColorSemiTransparent As New MyColor(1, Color8)

    Public ThemeNow As Integer = -1
    Public ColorHue As Integer = 210, ColorSat As Integer = 85, ColorLightAdjust As Integer = 0, ColorHueTopbarDelta As OneOf(Of Integer, Integer()) = 0
    Public ThemeDontClick As Integer = 0

    Public Sub ThemeRefresh(Optional NewTheme As Integer = -1)
        Try
            If ThemeNow = NewTheme AndAlso NewTheme >= 0 Then Return
            If NewTheme >= 0 Then ThemeNow = NewTheme

            '主题配色映射。「自定义」无限配色已移除（保留给作者），若存有旧值则回退到默认主题
            '合规：PCL 合理使用指南·重度使用第 5 条 —— 界面主要色相必须为蓝色，
            '因此无论保存过什么主题，一律回落到龙猫蓝（0），不再提供配色切换。
            If ThemeNow >= 0 Then ThemeNow = 0
            If ThemeNow >= 0 Then ThemeApplyPreset(ThemeNow)

            Color1 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.2, 25 + ColorLightAdjust * 0.3)
            Color2 = New MyColor().FromHSL2(ColorHue, ColorSat, 45 + ColorLightAdjust)
            Color3 = New MyColor().FromHSL2(ColorHue, ColorSat, 55 + ColorLightAdjust)
            Color4 = New MyColor().FromHSL2(ColorHue, ColorSat, 65 + ColorLightAdjust)
            Color5 = New MyColor().FromHSL2(ColorHue, ColorSat, 80 + ColorLightAdjust * 0.4)
            Color6 = New MyColor().FromHSL2(ColorHue, ColorSat, 91 + ColorLightAdjust * 0.1)
            Color7 = New MyColor().FromHSL2(ColorHue, ColorSat, 95)
            Color8 = New MyColor().FromHSL2(ColorHue, ColorSat, 97)
            ColorBg0 = Color4 * 0.4 + Color5 * 0.4 + ColorGray4 * 0.2
            ColorBg1 = New MyColor(190, Color7)

            ColorSemiTransparent = New MyColor(1, Color8)
            Application.Current.Resources("ColorBrush1") = New SolidColorBrush(Color1)
            Application.Current.Resources("ColorBrush2") = New SolidColorBrush(Color2)
            Application.Current.Resources("ColorBrush3") = New SolidColorBrush(Color3)
            Application.Current.Resources("ColorBrush4") = New SolidColorBrush(Color4)
            Application.Current.Resources("ColorBrush5") = New SolidColorBrush(Color5)
            Application.Current.Resources("ColorBrush6") = New SolidColorBrush(Color6)
            Application.Current.Resources("ColorBrush7") = New SolidColorBrush(Color7)
            Application.Current.Resources("ColorBrush8") = New SolidColorBrush(Color8)
            Application.Current.Resources("ColorBrushBg0") = New SolidColorBrush(ColorBg0)
            Application.Current.Resources("ColorBrushBg1") = New SolidColorBrush(ColorBg1)
            Application.Current.Resources("ColorObject1") = CType(Color1, Color)
            Application.Current.Resources("ColorObject2") = CType(Color2, Color)
            Application.Current.Resources("ColorObject3") = CType(Color3, Color)
            Application.Current.Resources("ColorObject4") = CType(Color4, Color)
            Application.Current.Resources("ColorObject5") = CType(Color5, Color)
            Application.Current.Resources("ColorObject6") = CType(Color6, Color)
            Application.Current.Resources("ColorObject7") = CType(Color7, Color)
            Application.Current.Resources("ColorObject8") = CType(Color8, Color)
            Application.Current.Resources("ColorObjectBg0") = CType(ColorBg0, Color)
            Application.Current.Resources("ColorObjectBg1") = CType(ColorBg1, Color)
            ThemeRefreshMain()
        Catch ex As Exception
            Logger.Error(ex, "刷新主题颜色失败", LogBehavior.Toast)
        End Try
    End Sub

    ''' <summary>
    ''' 主题 ID 到配色的映射。开源版原实现已被移除，此处为重新实现，
    ''' 色值按主题名称近似取定，与官方版的具体数值未必完全一致。
    ''' </summary>
    Public Sub ThemeApplyPreset(Id As Integer)
        Select Case Id
            Case 0  ' 龙猫蓝
                ColorHue = 210 : ColorSat = 85 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 1  ' 甜柠青
                ColorHue = 175 : ColorSat = 62 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 2  ' 小草绿
                ColorHue = 110 : ColorSat = 58 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 3  ' 菠萝黄
                ColorHue = 45 : ColorSat = 88 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 4  ' 橡木棕
                ColorHue = 30 : ColorSat = 45 : ColorLightAdjust = -5 : ColorHueTopbarDelta = 0
            Case 5  ' 玄素黑
                ColorHue = 210 : ColorSat = 0 : ColorLightAdjust = -12 : ColorHueTopbarDelta = 0
            Case 6  ' 铁杆粉
                ColorHue = 330 : ColorSat = 62 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 7  ' 神秘紫
                ColorHue = 272 : ColorSat = 68 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 8  ' 秋仪金
                ColorHue = 40 : ColorSat = 62 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 9  ' 活跃橙
                ColorHue = 24 : ColorSat = 88 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 10 ' 跳票红
                ColorHue = 355 : ColorSat = 72 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
            Case 11 ' 极客蓝
                ColorHue = 218 : ColorSat = 92 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 6
            Case 12 ' 滑稽彩：随时间轮转色相
                ColorHue = CInt(Environment.TickCount / 4000) Mod 360 : ColorSat = 85 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 20
            Case 13 ' 欧皇彩：每次刷新随机
                ColorHue = New Random().Next(0, 360) : ColorSat = 85 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 15
            Case Else
                ColorHue = 210 : ColorSat = 85 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
        End Select
    End Sub
    Public Sub ThemeRefreshMain()
        RunInUi(
        Sub()
            If Not FrmMain.IsLoaded Then Return
            '顶部条背景
            Dim Brush = New LinearGradientBrush With {.EndPoint = New Point(1, 0), .StartPoint = New Point(0, 0)}
            Dim Deltas = ColorHueTopbarDelta.Switch(Function(d) New Integer() {-d, 0, d}, Function(d) d)
            Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue + Deltas(0), ColorSat, 48 + ColorLightAdjust)})
            Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue + Deltas(1), ColorSat, 54 + ColorLightAdjust)})
            Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + Deltas(2), ColorSat, 48 + ColorLightAdjust)})
            FrmMain.PanTitle.Background = Brush
            FrmMain.PanTitle.Background.Freeze()
            '主页面背景
            If Settings.Get(Of Boolean)("UiBackgroundColorful") Then
                Brush = New LinearGradientBrush With {.EndPoint = New Point(0.1, 1), .StartPoint = New Point(0.9, 0)}
                Brush.GradientStops.Add(New GradientStop With {.Offset = -0.1, .Color = New MyColor().FromHSL2(ColorHue - 15, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.4, .Color = New MyColor().FromHSL2(ColorHue, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1.1, .Color = New MyColor().FromHSL2(ColorHue + 15, ColorSat * 0.8, 91)})
                FrmMain.PanForm.Background = Brush
            Else
                FrmMain.PanForm.Background = New MyColor(245, 245, 245)
            End If
            FrmMain.PanForm.Background.Freeze()
        End Sub)
    End Sub
    Friend Sub ThemeCheckAll(EffectSetup As Boolean)
    End Sub
    Friend Function ThemeCheckOne(Id As Integer) As Boolean
        Return True
    End Function
    Friend Function ThemeUnlock(Id As Integer, Optional ShowDoubleHint As Boolean = True, Optional UnlockHint As String = Nothing) As Boolean
        Return False
    End Function

#End Region

#Region "更新"

    '自建 OTA 服务地址，末尾必须保留斜杠
    Friend Const OtaBase As String = "https://pcl.umrca.com/pcl2/"

    '更新包在临时目录中的文件名。刻意与官方版区分，避免共用一个临时目录时互相覆盖
    Friend Const UpdateFileName As String = "LatestOta.exe"

    Friend IsUpdateWaitingRestart As Boolean = False

    ''' <summary>
    ''' 拉取 OTA 版本信息，同时刷新 Notice.cfg 供 IsLauncherNewest 判断。
    ''' </summary>
    Friend Function GetOtaVersion() As JObject
        Dim Raw As String = NetRequestByClientRetry(OtaBase & "version.json", RequireJson:=True)
        Dim Obj As JObject = JObject.Parse(Raw)
        '先验签，通过后才使用这份数据（防止中间人同时改写 url 与 sha256）
        VerifyVersionSignature(Obj)
        Try
            Dim Code As Integer = Obj("versionCode").ToObject(Of Integer)()
            Dim NoticeId As String = ""
            Try
                Dim NoticeRaw As String = NetRequestByClient(OtaBase & "notice.json", RequireJson:=False)
                If NoticeRaw.Contains("""title""") Then
                    NoticeId = JObject.Parse(NoticeRaw)("id").ToString()
                End If
            Catch ex As Exception
                Logger.Warn(ex, "获取公告失败")
            End Try
            'Notice.cfg 格式：快照版版本号|正式版版本号|公告ID
            FileUtils.Write(PathTemp & "Cache\Notice.cfg", $"{Code}|{Code}|{NoticeId}", Encoding.UTF8)
        Catch ex As Exception
            Logger.Warn(ex, "写入 Notice.cfg 失败")
        End Try
        Return Obj
    End Function

    ''' <summary>
    ''' 检查更新。有新版则询问、下载、校验 SHA256，然后重启自身完成替换。
    ''' </summary>
    Friend Sub UpdateCheckByButton()
        Hint("正在检查更新……")
        RunInNewThread(
            Sub()
                Try
                    Dim Obj As JObject = GetOtaVersion()
                    Dim Code As Integer = Obj("versionCode").ToObject(Of Integer)()
                    Dim Name As String = Obj("versionName").ToString()
                    Dim Changelog As String = If(Obj("changelog")?.ToString(), "").Trim()
                    Dim ForceUpdate As Boolean = Obj("force") IsNot Nothing AndAlso Obj("force").ToObject(Of Boolean)()

                    If Code <= VersionCode Then
                        RunInUiWait(Sub() Hint($"当前已是最新版本（{VersionBaseName}）"))
                        Return
                    End If

                    Dim Msg As String = $"发现新版本 {Name}（{Code}）{vbCrLf}当前版本 {VersionBaseName}（{VersionCode}）"
                    If Changelog <> "" Then Msg &= vbCrLf & vbCrLf & Changelog
                    If ForceUpdate Then Msg &= vbCrLf & vbCrLf & "此版本为强制更新，必须更新后才能继续使用。"

                    Dim Result As Integer
                    If ForceUpdate Then
                        Result = RunInUiWait(Function() MyMsgBox(Msg, "发现更新", "立即更新", "退出"))
                    Else
                        Result = RunInUiWait(Function() MyMsgBox(Msg, "发现更新", "立即更新", "稍后"))
                    End If

                    If Result <> 1 Then
                        If ForceUpdate Then RunInUiWait(Sub() FormMain.EndProgramForce(ProcessReturnValues.Cancel))
                        Return
                    End If

                    '下载更新包
                    Dim Latest As String = PathTemp & UpdateFileName
                    FileUtils.Delete(Latest)
                    RunInUiWait(Sub() Hint("正在下载更新包……"))
                    NetDownloadByClient(Obj("url").ToString(), Latest)

                    '大小校验
                    Dim ExpectSize As Long = If(Obj("size") IsNot Nothing, Obj("size").ToObject(Of Long)(), 0L)
                    Dim ActualSize As Long = FileUtils.GetInfo(Latest).Length
                    If ExpectSize > 0 AndAlso ActualSize <> ExpectSize Then
                        FileUtils.Delete(Latest)
                        Logger.Warn($"更新包大小不符：期望 {ExpectSize}，实际 {ActualSize}")
                        RunInUiWait(Sub() Hint("更新包大小不符，已取消更新"))
                        Return
                    End If

                    'SHA256 校验：exe 没有数字签名，这一步是防止被替换或下载损坏的关键
                    Dim ExpectSha As String = If(Obj("sha256")?.ToString(), "").Trim().ToLower()
                    If ExpectSha <> "" Then
                        Dim ActualSha As String = CryptographyUtils.ComputeFileHash(Latest, CryptographyUtils.HashMethod.Sha256).Trim().ToLower()
                        If ActualSha <> ExpectSha Then
                            FileUtils.Delete(Latest)
                            Logger.Warn($"更新包校验失败：期望 {ExpectSha}，实际 {ActualSha}")
                            RunInUiWait(Sub() Hint("更新包校验失败，已取消更新"))
                            Return
                        End If
                    Else
                        Logger.Warn("OTA 未提供 sha256，已跳过校验")
                    End If

                    Logger.Info($"更新包已就绪：{Latest}")
                    IsUpdateWaitingRestart = True
                    UpdateRestart(True)
                Catch ex As Exception
                    Logger.Error(ex, "检查更新失败")
                    RunInUiWait(Sub() Hint("检查更新失败：" & ex.Message))
                End Try
            End Sub, "检查更新")
    End Sub

    ''' <summary>
    ''' 启动更新程序并（按需）结束当前进程。更新程序会等本进程退出后替换文件。
    ''' </summary>
    Public Sub UpdateRestart(TriggerRestartAndByEnd As Boolean)
        Try
            Dim Latest As String = PathTemp & UpdateFileName
            If Not FileUtils.Exists(Latest) Then
                Logger.Warn("更新包不存在，无法启动更新程序")
                Return
            End If
            IsUpdateWaitingRestart = False
            Dim Args As String = $"--update {Process.GetCurrentProcess().Id} ""{PathExe}"" ""{Latest}"" True"
            Logger.Info($"启动更新程序：{Latest} {Args}")
            StartProcess(Latest, Args)
            If TriggerRestartAndByEnd Then RunInUiWait(Sub() FormMain.EndProgramForce())
        Catch ex As Exception
            Logger.Error(ex, "启动更新程序失败")
            RunInUiWait(Sub() Hint("启动更新程序失败：" & ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' 由新版本进程在启动参数为 --update 时调用：等旧进程退出，替换 exe，再按需重启。
    ''' </summary>
    Public Sub UpdateReplace(ProcessId As Integer, OldFileName As String, NewFileName As String, TriggerRestart As Boolean)
        Try
            '1. 等待旧进程退出
            Try
                Dim OldProc As Process = Process.GetProcessById(ProcessId)
                If Not OldProc.WaitForExit(15000) Then Logger.Warn("等待旧进程退出的时间已超过 15 秒")
            Catch ex As ArgumentException
                '进程已经不存在了，直接继续
            End Try
            Thread.Sleep(400)

            '2. 替换文件。旧进程刚退出时文件锁可能还没释放，这里做几次重试
            Dim Replaced As Boolean = False
            For i = 1 To 20
                Try
                    If FileUtils.Exists(OldFileName) Then FileUtils.Delete(OldFileName)
                    File.Copy(NewFileName, OldFileName, True)
                    Replaced = True
                    Exit For
                Catch ex As Exception
                    Logger.Warn($"替换启动器文件第 {i} 次失败，稍后重试：{ex.Message}")
                    Thread.Sleep(300)
                End Try
            Next

            If Not Replaced Then
                Logger.Error($"替换启动器文件失败：{NewFileName} → {OldFileName}")
                MsgBox("自动更新失败，请手动复制以下文件并覆盖原文件：" & vbCrLf &
                       NewFileName & vbCrLf & "→" & vbCrLf & OldFileName, MsgBoxStyle.Critical, "更新失败")
                Return
            End If
            Logger.Info($"已替换启动器文件：{OldFileName}")

            '3. 按需重启
            If TriggerRestart Then StartProcess(OldFileName)
        Catch ex As Exception
            Logger.Error(ex, "自动更新替换失败")
        End Try
    End Sub

    ''' <summary>
    ''' 确保 PathTemp 下的更新包是最新正式版的 PCL，它会被用于整合包打包。
    ''' 如果不是，则下载一个。
    ''' </summary>
    Friend Sub DownloadLatestPCL(Optional LoaderToSyncProgress As LoaderBase = Nothing)
        Dim Latest As String = PathTemp & UpdateFileName
        Try
            Dim Obj As JObject = GetOtaVersion()
            Dim ExpectSha As String = If(Obj("sha256")?.ToString(), "").Trim().ToLower()
            If FileUtils.Exists(Latest) AndAlso ExpectSha <> "" Then
                Dim ExistSha As String = CryptographyUtils.ComputeFileHash(Latest, CryptographyUtils.HashMethod.Sha256).Trim().ToLower()
                If ExistSha = ExpectSha Then Return
            End If
            FileUtils.Delete(Latest)
            NetDownloadByLoader(Obj("url").ToString(), Latest, LoaderToSyncProgress)
        Catch ex As Exception
            Logger.Error(ex, "下载最新版 PCL 失败")
            Throw
        End Try
    End Sub

#End Region

#Region "联网配置"

    ''' <summary>
    ''' 联网获取的配置信息。
    ''' 若获取失败或仍在获取中，可能为 Nothing。
    ''' </summary>
    Public ServerConfig As JObject

    Public ServerLoader As New LoaderTask(Of Integer, Integer)("PCL 配置更新", Sub() RefreshServerConfig(), Priority:=ThreadPriority.BelowNormal) With {.ReloadTimeout = 1000 * 60 * 60}

    ''' <summary>
    ''' 拉取启动器公告，只在与上次已读的不同时弹窗提示。
    ''' </summary>
    Friend Sub RefreshServerConfig()
        '联机（EasyTier）配置。
        '节点列表 API（uptime.easytier.cn）已下线，本修改版不内置任何公共中继地址，
        '而是由用户在「设置 → 联机 → 自定义节点」中填写。
        'Peers 留空时联机页会提示用户去填写，不会带着空地址强行启动 EasyTier。
        'EasyTier 为 LGPL-3.0，二进制直接从官方 GitHub Release 下载，不随本仓库分发。
        ServerConfig = New JObject(
            New JProperty("Link", New JObject(
                New JProperty("EasyTierVersion", 2),
                New JProperty("Downloads", New JArray(
                    "https://github.com/EasyTier/EasyTier/releases/download/v2.6.4/easytier-windows-{arch}-v2.6.4.zip")),
                New JProperty("Argument", ""),
                New JProperty("Peers", New JArray()),
                New JProperty("MandatoryPeers", New JArray()),
                New JProperty("PeersBlackList", New JArray()),
                New JProperty("RandomPeer", 0),
                New JProperty("DiscoverPeerId", 0),
                New JProperty("DiscoverPeer", ""))))
        Try
            Dim Raw As String = NetRequestByClient(OtaBase & "notice.json", RequireJson:=False)
            Dim Obj As JObject = JObject.Parse(Raw)
            If Obj("id") Is Nothing Then Return
            Dim Id As String = Obj("id").ToString()
            If Id = Settings.Get(Of String)("SystemLastNoticeId") Then Return
            Settings.Set("SystemLastNoticeId", Id)
            Dim Content As String = Obj("content").ToString()
            Dim Title As String = Obj("title").ToString()
            Logger.Info($"收到新公告 [{Id}]：{Title}")
            RunInUiWait(Sub() MyMsgBox(Content, "公告 · " & Title))
        Catch ex As Exception
            Logger.Warn(ex, "获取启动器公告失败")
        End Try
    End Sub

#End Region

#Region "赞助等级"

    Public ReadOnly Property CurrentRank As DonationRank
        Get
            Return DonationRank.None
        End Get
    End Property

    Public Sub InputPotatoCode(IsUpdating As Boolean)
    End Sub
    Friend Sub GeneratePotatoCode()
    End Sub

    ''' <summary>
    ''' 获取设备识别码。
    ''' </summary>
    Friend Function GetIdentify() As String
        Return "0000-0000-0000-0000"
    End Function

#End Region

#Region "版本签名验证"

    '更新清单签名公钥（RSA-2048）。私钥只存在于 OTA 服务端，不在客户端。
    Private Const OtaPubModulus As String =
        "jeUuU01gW2US0UwRtnhE6ZDNfzjtFVXdT4AbUilrF4dvkK4/FwoteSd6JZuwsmA+fPQ0Xm2Ao3bdNqZsNOjyR99EaI9PO+flRwSH4zR0ZKBDhvF4GC9I4F4TjGy44k21iEdxuWBJy3bpbJKNor+AGhUPo/pzW7sXKnm/G+dr+1iKqdBb/buqKoKuXlDCrV7dcji35Upvkxog91fEUVXX30DAiQ0HcvwiueMFW54O2M9oQst+71Bess7SZ9rjklqVvCdV4w/WhrdhYzvLYjHACRd8FOddBNigIwkjbWpkFIJVYsegSs2ITYMj2FbaqOP5Ob10cO2P4nfHc6gmdbwxPw=="
    Private Const OtaPubExponent As String = "AQAB"

    ''' <summary>
    ''' 校验 version.json 的数字签名。失败直接抛异常，调用方不会再使用这份数据。
    ''' 签名内容是与服务端约定的「字段以 chr(31) 连接」的规范字符串，
    ''' 刻意不签 JSON 原文——两侧重新序列化 JSON 极易因键序/空格/转义而失配。
    ''' </summary>
    Friend Sub VerifyVersionSignature(Obj As JObject)
        Dim SigText As String = If(Obj("sig")?.ToString(), "")
        If SigText.Trim() = "" Then
            Throw New Exception("更新信息缺少数字签名，已拒绝（服务端可能未配置签名私钥）")
        End If

        Dim Fields As String() = {
            Obj("versionCode").ToString(),
            Obj("versionName").ToString(),
            Obj("url").ToString(),
            Obj("sha256").ToString(),
            Obj("size").ToString(),
            If(Obj("force") IsNot Nothing AndAlso Obj("force").ToObject(Of Boolean)(), "1", "0"),
            If(Obj("changelog")?.ToString(), "")
        }
        Dim Canon As String = String.Join(Chr(31).ToString(), Fields)
        Dim Data As Byte() = Encoding.UTF8.GetBytes(Canon)
        VerifyRsaSignature(Data, SigText, "更新信息")
        Logger.Info("更新信息签名校验通过")
    End Sub

    ''' <summary>
    ''' 用内置公钥校验一段数据与签名。失败抛异常，调用方不再使用该数据。
    ''' </summary>
    Friend Sub VerifyRsaSignature(Data As Byte(), SigText As String, What As String)
        Dim Sig As Byte()
        Try
            Sig = Convert.FromBase64String(SigText.Trim())
        Catch ex As FormatException
            Throw New Exception(What & "的签名不是合法 Base64，已拒绝")
        End Try
        Dim Params As New System.Security.Cryptography.RSAParameters
        Params.Modulus = Convert.FromBase64String(OtaPubModulus)
        Params.Exponent = Convert.FromBase64String(OtaPubExponent)
        Dim Rsa As System.Security.Cryptography.RSA = System.Security.Cryptography.RSA.Create()
        Try
            Rsa.ImportParameters(Params)
            If Not Rsa.VerifyData(Data, Sig,
                                  System.Security.Cryptography.HashAlgorithmName.SHA256,
                                  System.Security.Cryptography.RSASignaturePadding.Pkcs1) Then
                Throw New Exception(What & "签名无效，已拒绝（可能被篡改或伪造）")
            End If
        Finally
            Rsa.Dispose()
        End Try
    End Sub

#End Region

#Region "授权"

    ''' <summary>
    ''' 真实设备标识。开源版的 GetIdentify 是固定常量，不能用来绑定设备，
    ''' 因此这里另取 MachineGuid 做哈希。
    ''' </summary>
    Friend Function GetDeviceId() As String
        Try
            Dim Raw As String = ""
            Try
                Using K = Microsoft.Win32.RegistryKey.OpenBaseKey(
                        Microsoft.Win32.RegistryHive.LocalMachine,
                        Microsoft.Win32.RegistryView.Registry64).OpenSubKey("SOFTWARE\Microsoft\Cryptography")
                    Raw = If(K?.GetValue("MachineGuid")?.ToString(), "")
                End Using
            Catch ex As Exception
                Logger.Warn(ex, "读取 MachineGuid 失败")
            End Try
            If Raw.Trim() = "" Then Raw = Environment.MachineName & "|" & Environment.UserName
            Return CryptographyUtils.ComputeHash(Raw, CryptographyUtils.HashMethod.Sha256).Substring(0, 16).ToUpper()
        Catch ex As Exception
            Logger.Error(ex, "生成设备标识失败")
            Return "UNKNOWN-DEVICE"
        End Try
    End Function

    ''' <summary>
    ''' 向 OTA 服务校验授权码。
    ''' 返回 1 通过、0 网络不可用、-1 被拒绝（原因写入 Message）。
    ''' </summary>
    Friend Function LicenseVerify(Key As String, ByRef Message As String) As Integer
        '每次请求用一次性随机码，服务端把它签进响应，旧的合法响应无法重放
        Dim Nonce As String = Guid.NewGuid().ToString("N")
        Dim Obj As JObject = Nothing

        '阶段一：只处理网络问题。连不上才算「不可达」，才允许走宽限期
        Try
            Dim Body As New JObject()
            Body("key") = Key.Trim()
            Body("device") = GetDeviceId()
            Body("version") = VersionCode
            Body("nonce") = Nonce
            Dim Resp As String = NetRequestByClient(OtaBase & "auth", HttpMethod.Post,
                                                    Body.ToString(Formatting.None), "application/json",
                                                    RequireJson:=True, Timeout:=15000)
            Obj = JObject.Parse(Resp)
        Catch ex As Exception
            Logger.Warn(ex, "授权服务器不可达")
            Message = "无法连接授权服务器"
            Return 0
        End Try

        '阶段二：响应内容校验。任何失败都算「明确拒绝」，绝不落入宽限期
        Try
            Dim Granted As Boolean = Obj("ok") IsNot Nothing AndAlso Obj("ok").ToObject(Of Boolean)()
            If Not Granted Then
                Message = If(Obj("message")?.ToString(), "授权码无效")
                Return -1
            End If

            '通过后必须验签：没有服务端私钥就伪造不出这一份响应
            Dim SigText As String = If(Obj("sig")?.ToString(), "")
            If SigText.Trim() = "" Then
                Logger.Warn("授权响应缺少签名，已拒绝")
                Message = "服务端未启用来电签名，请联系维护者"
                Return -1
            End If
            If If(Obj("nonce")?.ToString(), "") <> Nonce Then
                Logger.Warn("授权响应的 nonce 不匹配，已拒绝")
                Message = "授权响应校验失败（nonce）"
                Return -1
            End If
            If If(Obj("device")?.ToString(), "") <> GetDeviceId() Then
                Logger.Warn("授权响应的设备标识不匹配，已拒绝")
                Message = "授权响应校验失败（设备）"
                Return -1
            End If
            Dim Canon As String = String.Join(Chr(31).ToString(), {
                "auth",
                If(Granted, "1", "0"),
                If(Obj("expiresAt")?.ToString(), ""),
                If(Obj("serverTime")?.ToString(), ""),
                Obj("device").ToString(),
                Obj("nonce").ToString()
            })
            VerifyRsaSignature(Encoding.UTF8.GetBytes(Canon), SigText, "授权响应")

            Settings.Set("SystemLicenseLastOk", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            '记录有效期，供「设置 → 授权」页面展示（空字符串表示永久）
            Settings.Set("SystemLicenseExpires", If(Obj("expiresAt")?.ToString(), "").Trim())
            Return 1
        Catch ex As Exception
            Logger.Warn(ex, "授权响应校验失败：" & ex.Message)
            Message = "授权响应校验失败，请联系维护者"
            Return -1
        End Try
    End Function

    ''' <summary>距上次校验成功是否还在 7 天宽限期内。</summary>
    Friend Function LicenseWithinGrace() As Boolean
        Try
            Dim Raw As String = Settings.Get(Of String)("SystemLicenseLastOk")
            If Raw = "" Then Return False
            Dim Last As Long
            If Not Long.TryParse(Raw, Last) Then Return False
            Return (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Last) / 86400.0 <= 7.0
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' 启动时的授权闸门。已有授权且校验通过则直接放行；
    ''' 服务器不可达时给 7 天宽限；其余情况要求重新输入。
    ''' </summary>
    Friend Function LicenseCheck() As Boolean
        Dim Key As String = ""
        Try
            Key = Settings.Get(Of String)("SystemLicenseKey").Trim()
        Catch ex As Exception
            Logger.Warn(ex, "读取本地授权码失败")
        End Try

        If Key <> "" Then
            'PCL 的 NetClient 禁止在 UI 线程发起非 127.x 的请求（ModNet:219），
            '而本函数运行在 Application.OnStartup 的 UI 线程上。
            '之前只在本机 127.0.0.1 测试，恰好命中它的 localhost 豁免，所以问题被掩盖。
            '这里把网络请求放到后台线程执行，UI 线程阻塞等待，避免误判成「服务器不可达」。
            Dim Result As Integer = 0
            Dim Msg As String = ""
            Dim Bg As New Threading.Thread(
                Sub()
                    Try
                        Result = LicenseVerify(Key, Msg)
                    Catch ex As Exception
                        Logger.Warn(ex, "授权校验线程异常")
                        Result = 0
                    End Try
                End Sub)
            Bg.IsBackground = True
            Bg.Start()
            If Not Bg.Join(20000) Then
                Logger.Warn("授权校验超时（20 秒）")
                Result = 0
            End If
            Select Case Result
                Case 1
                    Logger.Info($"授权校验通过（设备 {GetDeviceId()}）")
                    Return True
                Case -1
                    Logger.Warn($"授权被拒绝：{Msg}")
                    Settings.Set("SystemLicenseKey", "")
                    Key = ""
                Case Else
                    If LicenseWithinGrace() Then
                        Logger.Info("授权服务器不可达，处于 7 天宽限期内，继续启动")
                        Return True
                    End If
                    Logger.Warn("授权服务器不可达且已超出宽限期")
            End Select
        End If

        Return ShowLicenseDialog(Key)
    End Function

    ''' <summary>授权输入窗口。校验成功返回 True。</summary>
    Friend Function ShowLicenseDialog(Initial As String) As Boolean
        Try
            'CenterScreen 在多显示器/HiDPI 下会算偏，这里按主屏工作区手动居中
            Dim WA = SystemParameters.WorkArea
            Dim WinW As Double = 486, WinH As Double = 306
            Dim Win As New Window With {
                .Title = "PCL 授权验证",
                .Width = WinW,
                .Height = WinH,
                .ResizeMode = ResizeMode.NoResize,
                .WindowStartupLocation = WindowStartupLocation.Manual,
                .Left = WA.Left + (WA.Width - WinW) / 2,
                .Top = WA.Top + (WA.Height - WinH) / 2,
                .Background = New SolidColorBrush(Color.FromRgb(244, 247, 252))
            }
            Dim Root As New StackPanel With {.Margin = New Thickness(28, 22, 28, 18)}

            Root.Children.Add(New TextBlock With {
                .Text = "需要授权",
                .FontSize = 19,
                .FontWeight = FontWeights.SemiBold,
                .Foreground = New SolidColorBrush(Color.FromRgb(20, 30, 48))
            })
            Root.Children.Add(New TextBlock With {
                .Text = "本程序为个人修改版，需要授权码才能使用。请向维护者索取；首次校验会绑定当前设备。",
                .TextWrapping = TextWrapping.Wrap,
                .FontSize = 12.5,
                .Margin = New Thickness(0, 9, 0, 0),
                .Foreground = New SolidColorBrush(Color.FromRgb(100, 114, 138))
            })

            Dim Box As New TextBox With {
                .Text = Initial,
                .FontSize = 14,
                .FontFamily = New FontFamily("Consolas"),
                .Padding = New Thickness(10, 8, 10, 8),
                .Margin = New Thickness(0, 16, 0, 0),
                .VerticalContentAlignment = VerticalAlignment.Center,
                .CharacterCasing = CharacterCasing.Upper
            }
            Root.Children.Add(Box)

            Dim Err As New TextBlock With {
                .Text = "",
                .TextWrapping = TextWrapping.Wrap,
                .FontSize = 12.5,
                .MinHeight = 34,
                .Margin = New Thickness(0, 10, 0, 0),
                .Foreground = New SolidColorBrush(Color.FromRgb(193, 43, 43))
            }
            Root.Children.Add(Err)

            Dim Btns As New StackPanel With {
                .Orientation = Orientation.Horizontal,
                .HorizontalAlignment = HorizontalAlignment.Right,
                .Margin = New Thickness(0, 6, 0, 0)
            }
            Dim BtnOk As New Button With {.Content = "验证并进入", .Width = 116, .Height = 34, .IsDefault = True}
            Dim BtnExit As New Button With {.Content = "退出", .Width = 84, .Height = 34, .Margin = New Thickness(10, 0, 0, 0), .IsCancel = True}
            Btns.Children.Add(BtnOk)
            Btns.Children.Add(BtnExit)
            Root.Children.Add(Btns)
            Win.Content = Root

            AddHandler BtnOk.Click,
                Sub()
                    Dim Key As String = Box.Text.Trim()
                    If Key = "" Then
                        Err.Text = "请先输入授权码"
                        Return
                    End If
                    Err.Foreground = New SolidColorBrush(Color.FromRgb(100, 114, 138))
                    Err.Text = "正在校验……"
                    'PCL 的 NetClient 禁止在 UI 线程发起非 127.x 的请求（ModNet:219）。
                    '按钮点击事件跑在 UI 线程上，直接调用必然抛异常，表现为「无法连接授权服务器」。
                    '因此放到后台线程请求，完成后用 Dispatcher 回到 UI 线程更新界面（不冻结窗口）。
                    Logger.Info($"授权：提交校验，密钥长度 {Key.Length}，设备 {GetDeviceId()}")
                    BtnOk.IsEnabled = False
                    Dim Bg As New Threading.Thread(
                        Sub()
                            Dim Msg As String = ""
                            Dim VerifyResult As Integer = 0
                            Try
                                VerifyResult = LicenseVerify(Key, Msg)
                            Catch ex As Exception
                                Logger.Warn(ex, "授权校验线程异常")
                                VerifyResult = 0
                            End Try
                            Win.Dispatcher.BeginInvoke(New Action(
                                Sub()
                                    BtnOk.IsEnabled = True
                                    Logger.Info($"授权：校验结果 {VerifyResult}，消息 {Msg}")
                                    Select Case VerifyResult
                                        Case 1
                                            Settings.Set("SystemLicenseKey", Key)
                                            Win.DialogResult = True
                                        Case 0
                                            Err.Foreground = New SolidColorBrush(Color.FromRgb(193, 43, 43))
                                            Err.Text = "无法连接授权服务器，请检查网络后重试"
                                        Case Else
                                            Err.Foreground = New SolidColorBrush(Color.FromRgb(193, 43, 43))
                                            Err.Text = Msg
                                    End Select
                                End Sub))
                        End Sub)
                    Bg.IsBackground = True
                    Bg.Start()
                End Sub
            AddHandler BtnExit.Click,
                Sub()
                    Logger.Warn("授权：用户选择退出")
                    Win.DialogResult = False
                End Sub

            Box.Focus()
            Logger.Info($"授权：显示输入窗口（设备 {GetDeviceId()}）")
            Dim DialogResultValue As Boolean = CBool(Win.ShowDialog())
            Logger.Info($"授权：窗口已关闭，结果 {DialogResultValue}")
            Return DialogResultValue
        Catch ex As Exception
            Logger.Error(ex, "显示授权窗口失败，改用系统输入框")
            Try
                Dim Typed As String = Microsoft.VisualBasic.InputBox("请输入授权码：", "PCL 授权验证", Initial)
                If Typed Is Nothing OrElse Typed.Trim() = "" Then Return False
                Dim Msg As String = ""
                If LicenseVerify(Typed, Msg) = 1 Then
                    Settings.Set("SystemLicenseKey", Typed.Trim())
                    Return True
                End If
                MyMsgBox(Msg, "授权失败")
            Catch ex2 As Exception
                Logger.Error(ex2, "备用授权输入也失败了")
            End Try
            Return False
        End Try
    End Function

#End Region

End Module
