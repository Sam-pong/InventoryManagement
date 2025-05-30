Imports System.Data
Imports System.Windows.Media.Animation
Imports System.Data.SQLite
Class MainWindow
    Public Property LoadedRefNumbers As New List(Of String)
    Dim AllTransactionRows As New DataTable
    Public Property CurrentIndex As Integer = 0

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim NewItemWin As New NewItemUC
        maingrid.Children.Clear()

        maingrid.Children.Add(NewItemWin)
    End Sub

    Private Sub Button_Click2(sender As Object, e As RoutedEventArgs)
        Dim edititemwin As New EditItemWindow
        edititemwin.ShowDialog()
    End Sub
    Private Sub Button3_Click(sender As Object, e As RoutedEventArgs)
        Dim newtrans As New TransactionEntryUC
        maingrid.Children.Clear()
        Dim result As Boolean? = maingrid.Children.Add(newtrans)
        If result.HasValue Then
            reloadrecents()
        End If
    End Sub


    Private Sub window_loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        maindockpanel.Width = 87


        Using con As New SQLiteConnection("Data Source=InvenManage.db")
            Dim sqlstring As String = "SELECT SN, REFNUMBER, DATE AS 'DATER', CUSTOMER, TOTALAMOUNT, SUBTOTAL, DISCOUNTTOTAL, TAXAMOUNT, TRANSTYPE, NOTES, STATUS  FROM TRANSACTIONS ORDER BY SN DESC LIMIT 10"
            con.Open()

            Using ad As New SQLiteDataAdapter(sqlstring, con)
                ad.Fill(AllTransactionRows)
                dtgrid.ItemsSource = AllTransactionRows.DefaultView
            End Using
        End Using

        For Each row As DataRow In AllTransactionRows.Rows
            viewnexttransaction()
        Next
    End Sub

    Sub reloadrecents()
        AllTransactionRows.Rows.Clear()
        LoadedRefNumbers.Clear()

        CurrentIndex = 0

        Using con As New SQLiteConnection("Data Source=InvenManage.db")
            Dim sqlstring As String = "SELECT * FROM TRANSACTIONS ORDER BY SN DESC LIMIT 10"
            con.Open()

            Using ad As New SQLiteDataAdapter(sqlstring, con)

                ad.Fill(AllTransactionRows)
                dtgrid.ItemsSource = AllTransactionRows.DefaultView
            End Using
        End Using


        For Each row As DataRow In AllTransactionRows.Rows
            viewnexttransaction()
        Next


    End Sub


    Public Sub viewnexttransaction()
        If CurrentIndex >= AllTransactionRows.Rows.Count Then
            Return
        End If

        Dim ref As String = AllTransactionRows.Rows(CurrentIndex)("REFNUMBER").ToString()
        If LoadedRefNumbers.Contains(ref) Then
            CurrentIndex += 1
            viewnexttransaction()
            Return
        End If



        LoadedRefNumbers.Add(ref)
        CurrentIndex += 1

    End Sub

End Class
