Imports System.Diagnostics
Imports System.Net.Sockets
Imports System.Threading.Tasks
Imports System.Windows.Media
Imports System.Windows.Threading

Public Class PageSetupLink

    Private Sub PageSetupLink_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        '重复加载部分
        PanBack.ScrollToHome()

        '非重复加载部分
        Static IsLoaded As Boolean = False
        If IsLoaded Then Return
        IsLoaded = True

        AniControlEnabled += 1
        Reload()
        AniControlEnabled -= 1

        '节点延迟：进页面先测一次，之后每 10 秒一次
        RefreshLatency()
        If _pingTimer Is Nothing Then
            _pingTimer = New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(10)}
            AddHandler _pingTimer.Tick, Sub() RefreshLatency()
        End If
        _pingTimer.Start()

    End Sub

    Private Sub PageSetupLink_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
        If _pingTimer IsNot Nothing Then _pingTimer.Stop()
    End Sub
    Public Sub Reload()
        SettingService.RefreshSettings(Me)
        RefreshNodeList()
    End Sub

    '==========================================================
    '  节点列表与延迟探测
    '==========================================================

    Private ReadOnly _latencyLabels As New List(Of TextBlock)
    Private ReadOnly _nodeAddresses As New List(Of String)
    Private _pingTimer As DispatcherTimer

    ''' <summary>按 ServerConfig 里的 Nodes 重建列表。</summary>
    Private Sub RefreshNodeList()
        If PanNodes Is Nothing Then Return
        PanNodes.Children.Clear()
        _latencyLabels.Clear()
        _nodeAddresses.Clear()

        Dim Nodes As List(Of JObject) = LinkNodes()
        '下拉框是附加功能，构建失败也不该影响节点列表与页面本身
        Try
            RefreshNodeCombo(Nodes)
        Catch ex As Exception
            Logger.Warn(ex, "构建「使用节点」下拉框失败")
        End Try
        If Nodes.Count = 0 Then
            PanNodes.Children.Add(New TextBlock With {
                .Text = "没有内置节点", .Opacity = 0.6, .Margin = New Thickness(0, 6, 0, 0)})
            Return
        End If

        For Each Node As JObject In Nodes
            Dim NodeName As String = If(Node?("name")?.ToString(), "未命名节点")
            Dim Addr As String = If(Node?("address")?.ToString(), "").Trim
            Dim RawExp As String = If(Node?("expiresAt")?.ToString(), "").Trim
            Dim Expired As Boolean = IsLinkNodeExpired(Node)

            Dim Row As New Grid With {.Margin = New Thickness(0, 5, 0, 0)}
            Row.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(2, GridUnitType.Star)})
            Row.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(2.4, GridUnitType.Star)})
            Row.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

            Dim TbName As New TextBlock With {
                .Text = NodeName, .VerticalAlignment = VerticalAlignment.Center,
                .TextTrimming = TextTrimming.CharacterEllipsis}
            If Expired Then TbName.Opacity = 0.45
            Grid.SetColumn(TbName, 0)

            Dim ExpText As String
            If RawExp.Length = 0 Then
                ExpText = "无期限"
            ElseIf Expired Then
                ExpText = RawExp & "（已过期）"
            Else
                ExpText = RawExp
            End If
            Dim TbExp As New TextBlock With {
                .Text = ExpText, .VerticalAlignment = VerticalAlignment.Center,
                .Opacity = 0.75, .FontSize = 12, .TextTrimming = TextTrimming.CharacterEllipsis}
            If Expired Then TbExp.Foreground = New SolidColorBrush(Color.FromRgb(200, 70, 70))
            Grid.SetColumn(TbExp, 1)

            Dim TbPing As New TextBlock With {
                .Text = If(Expired, "—", "检测中…"), .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Right, .FontSize = 12, .Opacity = 0.85}
            Grid.SetColumn(TbPing, 2)

            Row.Children.Add(TbName)
            Row.Children.Add(TbExp)
            Row.Children.Add(TbPing)
            PanNodes.Children.Add(Row)

            _latencyLabels.Add(TbPing)
            _nodeAddresses.Add(If(Expired, "", Addr))
        Next
    End Sub

    ''' <summary>每 10 秒跑一次，逐个测 TCP 握手耗时。</summary>
    Private Async Sub RefreshLatency()
        For i = 0 To _nodeAddresses.Count - 1
            If i >= _latencyLabels.Count Then Exit For
            Dim Addr As String = _nodeAddresses(i)
            Dim Label As TextBlock = _latencyLabels(i)
            If String.IsNullOrEmpty(Addr) Then
                Label.Text = "—"
                Continue For
            End If
            Dim Ms As Integer = Await MeasureTcpAsync(Addr, 2500)
            If Ms < 0 Then
                Label.Text = "超时"
                Label.Foreground = New SolidColorBrush(Color.FromRgb(200, 70, 70))
            Else
                Label.Text = Ms & " ms"
                Label.Foreground = New SolidColorBrush(
                    If(Ms < 100, Color.FromRgb(60, 160, 80),
                       If(Ms < 250, Color.FromRgb(200, 140, 40), Color.FromRgb(200, 70, 70))))
            End If
        Next
    End Sub

    ''' <summary>对 tcp://host:port 做一次 TCP 连接计时，超时返回 -1。</summary>
    Private Async Function MeasureTcpAsync(Address As String, TimeoutMs As Integer) As Task(Of Integer)
        Dim Host As String = ""
        Dim Port As Integer = 11010
        Try
            Dim S As String = Address.Replace("tcp://", "").Replace("udp://", "").Trim
            Dim Idx As Integer = S.LastIndexOf(":"c)
            If Idx > 0 Then
                Host = S.Substring(0, Idx).Trim
                Integer.TryParse(S.Substring(Idx + 1).Trim, Port)
            Else
                Host = S
            End If
            If Host.Length = 0 Then Return -1
            If Port <= 0 Then Port = 11010
        Catch
            Return -1
        End Try
        Dim Sw As Stopwatch = Stopwatch.StartNew()
        Try
            Using Client As New TcpClient()
                Dim ConnTask As Task = Client.ConnectAsync(Host, Port)
                Dim Done As Task = Await Task.WhenAny(ConnTask, Task.Delay(TimeoutMs))
                If Done IsNot ConnTask Then Return -1
                Await ConnTask
                Return CInt(Sw.ElapsedMilliseconds)
            End Using
        Catch
            Return -1
        End Try
    End Function

    '==========================================================
    '  节点选择
    '==========================================================

    Private _nodeComboLoading As Boolean = False
    Private ReadOnly _comboAddresses As New List(Of String)

    ''' <summary>重建「使用节点」下拉框。索引 0 是「自动」，其后每个可用节点一项。</summary>
    Private Sub RefreshNodeCombo(Nodes As List(Of JObject))
        If ComboNode Is Nothing Then Return
        _nodeComboLoading = True
        Try
            ComboNode.Items.Clear()
            _comboAddresses.Clear()
            ComboNode.Items.Add(New MyComboBoxItem With {.Content = "自动（所有可用节点）"})
            _comboAddresses.Add("")
            Dim Selected As String = LinkSelectedNode()
            Dim SelectedIndex As Integer = 0
            For Each Node As JObject In Nodes
                Dim Addr As String = If(Node?("address")?.ToString(), "").Trim
                If Addr.Length = 0 Then Continue For
                Dim ItemText As String = If(Node?("name")?.ToString(), Addr)
                If IsLinkNodeExpired(Node) Then ItemText &= "（已过期）"
                ComboNode.Items.Add(New MyComboBoxItem With {.Content = ItemText})
                _comboAddresses.Add(Addr)
                If Selected.Length > 0 AndAlso
                   Addr.Equals(Selected, StringComparison.OrdinalIgnoreCase) Then
                    SelectedIndex = _comboAddresses.Count - 1
                End If
            Next
            ComboNode.SelectedIndex = SelectedIndex
        Finally
            _nodeComboLoading = False
        End Try
    End Sub

    ''' <summary>选定某个节点就记住它；「自动」记为空字符串。</summary>
    Private Sub ComboNode_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboNode.SelectionChanged
        If _nodeComboLoading Then Return
        Dim Index As Integer = ComboNode.SelectedIndex
        If Index < 0 OrElse Index >= _comboAddresses.Count Then Return
        Dim Addr As String = _comboAddresses(Index)
        If Addr = LinkSelectedNode() Then Return
        SetLinkSelectedNode(Addr)
        Hint(If(Addr.Length = 0, "已改为自动选择节点", "已选定节点，下次进入联机页面时生效"),
             HintType.Green, False)
    End Sub

    '初始化
    Public Sub Reset()
        Try
            SettingService.ResetSettings(Me)
            Logger.Info("已初始化联机页设置")
            Hint("已初始化联机页设置！", HintType.Green, False)
        Catch ex As Exception
            Logger.Error(ex, "初始化联机页设置失败", LogBehavior.Alert)
        End Try
        Reload()
    End Sub

End Class
