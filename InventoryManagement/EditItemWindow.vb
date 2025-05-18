Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Data.SQLite
Class EditItemWindow

    Private items As ObservableCollection(Of Item)
    Private View As ICollectionView

    Sub tagfill()
        TAGCOMBO.ItemsSource = Nothing
        TAGCOMBO.Items.Clear()

        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            Using com As New SQLiteCommand("SELECT * FROM TAGS", con)
                con.Open()

                Try
                    Dim dt As New DataTable
                    Dim ad As New SQLiteDataAdapter(com)
                    ad.Fill(dt)
                    TAGCOMBO.ItemsSource = dt.DefaultView
                    TAGCOMBO.DisplayMemberPath = "TAGNAME"
                    TAGCOMBO.SelectedValuePath = "TAGID"
                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try

            End Using
            con.Close()

        End Using
    End Sub


    Private Function LoadItemsFromDatabase() As ObservableCollection(Of Item)
        Dim items As New ObservableCollection(Of Item)()

        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            con.Open()
            Dim sql As String = "SELECT SerielNumber, ItemName, Desc, Barcode, Location, Stock, TagName, PurchasePrice, RetailPrice FROM Items INNER JOIN TAGS ON Items.TAGID = TAGS.TAGID"
            Using cmd As New SQLiteCommand(sql, con)
                Using reader As SQLiteDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        items.Add(New Item With {
                            .ID = Convert.ToInt32(reader("SerielNumber")),
                            .Name = reader("ItemName").ToString(),
                            .Description = reader("Desc").ToString,
                                                        .Tag = reader("TagName"),
                            .Barcode = reader("Barcode").ToString,
                            .Location = reader("Location").ToString,
                            .Stock = Convert.ToInt32(reader("Stock")),
                            .BuyingPrice = Convert.ToDecimal(reader("PurchasePrice")),
                            .SellingPrice = Convert.ToDecimal(reader("RetailPrice"))
                                  }
                        )
                    End While
                End Using
            End Using
        End Using

        Return items
    End Function

    Private Function FilterItems(obj As Object) As Boolean
        Dim item As Item = TryCast(obj, Item)
        If item Is Nothing Then Return False

        If Not String.IsNullOrWhiteSpace(nametxt.Text) AndAlso
        Not item.Name.ToLower().Contains(nametxt.Text.ToLower()) Then Return False

        If Not String.IsNullOrWhiteSpace(desctxt.Text) AndAlso
        Not item.Description.ToLower().Contains(desctxt.Text.ToLower()) Then Return False

        If Not String.IsNullOrWhiteSpace(barcodetxt.Text) AndAlso
        Not item.Barcode.ToLower().Contains(barcodetxt.Text.ToLower()) Then Return False

        If Not String.IsNullOrWhiteSpace(locationtxt.Text) AndAlso
        Not item.Location.ToLower().Contains(locationtxt.Text.ToLower()) Then Return False

        If Not String.IsNullOrWhiteSpace(stocktxt.Text) AndAlso
        Not item.Stock.ToString().Contains(stocktxt.Text) Then Return False

        If TAGCOMBO.SelectedValue IsNot Nothing AndAlso
   Not String.IsNullOrWhiteSpace(TAGCOMBO.SelectedValue.ToString()) AndAlso
   Not item.Tag.ToString().Contains(TAGCOMBO.SelectedValue.ToString()) Then
            Return False
        End If
        If Not String.IsNullOrWhiteSpace(ItemID.Text) AndAlso
        Not item.ID.ToString().Contains(ItemID.Text) Then Return False

        If Not String.IsNullOrWhiteSpace(purchasetxt.Text) AndAlso
        Not item.BuyingPrice.ToString().Contains(purchasetxt.Text) Then Return False

        If Not String.IsNullOrWhiteSpace(retailtxt.Text) AndAlso
        Not item.SellingPrice.ToString().Contains(retailtxt.Text) Then Return False

        Return True
    End Function


    Private Sub Edititemwindow_loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        SearchResult.ItemsSource = Nothing

        SearchResult.Items.Clear()
        items = LoadItemsFromDatabase()

        View = CollectionViewSource.GetDefaultView(items)
        View.Filter = AddressOf FilterItems

        SearchResult.ItemsSource = View
        SearchResult.Columns(5).Visibility = Visibility.Collapsed
        SearchResult.Columns(6).Visibility = Visibility.Collapsed
        SearchResult.Columns(7).Visibility = Visibility.Collapsed
        SearchResult.Columns(8).Visibility = Visibility.Collapsed

        tagfill()
    End Sub



    Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles nametxt.TextChanged, ItemID.TextChanged, purchasetxt.TextChanged, stocktxt.TextChanged, locationtxt.TextChanged, retailtxt.TextChanged, barcodetxt.TextChanged, desctxt.TextChanged
        If View IsNot Nothing Then
            View.Refresh()
        End If
    End Sub

    Private Sub Button3_click(sender As Object, e As RoutedEventArgs)
        Me.Close()


    End Sub
End Class
