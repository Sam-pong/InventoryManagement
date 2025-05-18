Class MainWindow
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim NewItemWin As New NewItemWindow
        NewItemWin.ShowDialog()
    End Sub

    Private Sub Button_Click2(sender As Object, e As RoutedEventArgs)
        Dim edititemwin As New EditItemWindow
        edititemwin.ShowDialog()
    End Sub
End Class
