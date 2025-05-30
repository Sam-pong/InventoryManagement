Imports System.Data.SQLite
Imports System.Data
Imports System.Windows.Threading
Public Class Recentsmodule

    Dim mainWindow As MainWindow = CType(Window.GetWindow(Me), MainWindow)
    Public Property Refnumber As String

    Public Event ButtonTriggerRequested()
    Public Event LabelChangeRequested(ByVal newText As String)

    Public Sub New()
        InitializeComponent()
    End Sub




    Private Sub Button1_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
        e.Handled = False
    End Sub
    Private Sub uc_loaded(Sender As Object, e As EventArgs) Handles Me.Loaded

        Dispatcher.Invoke(Sub()
                              mainbuttonuc.InvalidateVisual()
                          End Sub, DispatcherPriority.Render)

        Using con As New SQLiteConnection("Data Source=InvenManage.db")
            Dim sqlstring As String = "SELECT * FROM TRANSACTIONS WHERE REFNUMBER = @refnumber"
            Using cmd As New SQLiteCommand(sqlstring, con)
                cmd.Parameters.AddWithValue("@refnumber", Refnumber)
                con.Open()

                Using reader As SQLiteDataReader = cmd.ExecuteReader()

                    If reader.Read() Then
                        If reader("TRANSTYPE") = 0 Then
                            Amountlbl.Foreground = Brushes.Red
                            Dim number As Decimal = Convert.ToDecimal(reader("TOTALAMOUNT"))
                            Dim formattedNumber As String = number.ToString("#,##0.00")
                            Amountlbl.Content = "-" & formattedNumber
                        Else
                            Amountlbl.Foreground = Brushes.Lime
                            Dim number As Decimal = Convert.ToDecimal(reader("TOTALAMOUNT"))
                            Dim formattedNumber As String = number.ToString("#,##0.00")
                            Amountlbl.Content = "+" & formattedNumber

                        End If
                        customernmlbl.Content = reader("CUSTOMER").ToString
                        RefNumberlbl.Content = reader("REFNUMBER").ToString
                        Dim dateString As String = reader("DATE").ToString
                        Dim parsedDate As DateTime = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", Globalization.CultureInfo.InvariantCulture)
                        Dim formattedDate As String = parsedDate.ToString("dd/MM/yyyy")
                        [date].Content = formattedDate

                    End If
                End Using
            End Using



        End Using

    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim transactionwindow As New TransactionEntry
        transactionwindow.ShowDialog()
        MsgBox("IT DOES WORK")


        RaiseEvent ButtonTriggerRequested()
        RaiseEvent LabelChangeRequested(RefNumberlbl.Content)
    End Sub
End Class
