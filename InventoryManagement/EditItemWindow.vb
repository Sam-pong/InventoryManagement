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
            Dim sql As String = "SELECT SerielNumber, ItemName, Desc, Barcode, Location, Stock, TagName, PurchasePrice, RetailPrice, Items.TagID FROM Items INNER JOIN TAGS ON Items.TAGID = TAGS.TAGID"
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
                            .SellingPrice = Convert.ToDecimal(reader("RetailPrice")),
                            .TagID = Convert.ToDecimal(reader("TagID"))
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
   Not item.TagID.ToString().Contains(TAGCOMBO.SelectedValue.ToString()) Then
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
        SearchResult.Columns(9).Visibility = Visibility.Collapsed

        SearchResult.Columns(0).Width = 35 ' SerielNumber
        SearchResult.Columns(1).Width = 150 ' ItemName
        SearchResult.Columns(2).Width = 180 ' Desc
        SearchResult.Columns(3).Width = 135 ' Barcode
        SearchResult.Columns(4).Width = 120 ' TagName

        tagfill()
        clearbut.Visibility = Visibility.Collapsed
        delbutton.Visibility = Visibility.Collapsed
        updatebutton.Visibility = Visibility.Collapsed

    End Sub



    Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles nametxt.TextChanged, ItemID.TextChanged, purchasetxt.TextChanged, stocktxt.TextChanged, locationtxt.TextChanged, retailtxt.TextChanged, barcodetxt.TextChanged, desctxt.TextChanged
        If View IsNot Nothing Then
            View.Refresh()
        End If
    End Sub
    Private Sub combochange(sender As Object, e As RoutedEventArgs) Handles TAGCOMBO.SelectionChanged
        If View IsNot Nothing Then
            View.Refresh()
        End If
    End Sub

    Private Sub Button3_click(sender As Object, e As RoutedEventArgs)
        Me.Close()


    End Sub


    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        If e.Key = Key.Escape Then
            Me.Close()
        End If
    End Sub

    Private Sub datagridselectionchange(sender As Object, e As RoutedEventArgs) Handles SearchResult.SelectionChanged
        If SearchResult.SelectedItem IsNot Nothing Then
            Dim selectedItem As Item = CType(SearchResult.SelectedItem, Item)

            ItemID.Text = selectedItem.ID
            nametxt.Text = selectedItem.Name
            desctxt.Text = selectedItem.Description
            TAGCOMBO.SelectedValue = selectedItem.TagID
            locationtxt.Text = selectedItem.Location
            stocktxt.Text = selectedItem.Stock
            purchasetxt.Text = selectedItem.BuyingPrice
            retailtxt.Text = selectedItem.SellingPrice
            barcodetxt.Text = selectedItem.Barcode
            clearbut.Visibility = Visibility.Visible
            delbutton.Visibility = Visibility.Visible
            updatebutton.Visibility = Visibility.Visible

        End If


    End Sub

    Private Sub Clearbutton_Click(sender As Object, e As RoutedEventArgs) Handles clearbut.Click
        ItemID.Text = Nothing
        nametxt.Text = Nothing
        desctxt.Text = Nothing
        TAGCOMBO.SelectedIndex = -1
        locationtxt.Text = Nothing
        stocktxt.Text = Nothing
        purchasetxt.Text = Nothing
        retailtxt.Text = Nothing
        barcodetxt.Text = Nothing
        clearbut.Visibility = Visibility.Collapsed
        delbutton.Visibility = Visibility.Collapsed
        updatebutton.Visibility = Visibility.Collapsed



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
        SearchResult.Columns(9).Visibility = Visibility.Collapsed

        SearchResult.Columns(0).Width = 35 ' SerielNumber
        SearchResult.Columns(1).Width = 150 ' ItemName
        SearchResult.Columns(2).Width = 180 ' Desc
        SearchResult.Columns(3).Width = 135 ' Barcode
        SearchResult.Columns(4).Width = 120 ' TagName

        tagfill()
        clearbut.Visibility = Visibility.Collapsed
        delbutton.Visibility = Visibility.Collapsed
        updatebutton.Visibility = Visibility.Collapsed



    End Sub

    Private Sub deletebutton_click(sender As Object, e As RoutedEventArgs) Handles delbutton.Click
        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            Using com As New SQLiteCommand("DELETE FROM ITEMS WHERE SerielNumber = @SN", con)
                con.Open()
                com.Parameters.AddWithValue("@SN", ItemID.Text)
                Try
                    com.ExecuteNonQuery()
                    MsgBox("Successfully deleted item: " & nametxt.Text)
                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try

            End Using
            con.Close()

        End Using



        ItemID.Text = Nothing
        nametxt.Text = Nothing
        desctxt.Text = Nothing
        TAGCOMBO.SelectedIndex = -1
        locationtxt.Text = Nothing
        stocktxt.Text = Nothing
        purchasetxt.Text = Nothing
        retailtxt.Text = Nothing
        barcodetxt.Text = Nothing
        clearbut.Visibility = Visibility.Collapsed
        delbutton.Visibility = Visibility.Collapsed
        updatebutton.Visibility = Visibility.Collapsed



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
        SearchResult.Columns(9).Visibility = Visibility.Collapsed

        SearchResult.Columns(0).Width = 35 ' SerielNumber
        SearchResult.Columns(1).Width = 150 ' ItemName
        SearchResult.Columns(2).Width = 180 ' Desc
        SearchResult.Columns(3).Width = 135 ' Barcode
        SearchResult.Columns(4).Width = 120 ' TagName

        tagfill()
        clearbut.Visibility = Visibility.Collapsed
        delbutton.Visibility = Visibility.Collapsed
        updatebutton.Visibility = Visibility.Collapsed


    End Sub
    Private Sub updatebutton_click(sender As Object, e As RoutedEventArgs) Handles updatebutton.Click
        If nametxt.Text = Nothing Then
            nametxt.Focus()
        ElseIf desctxt.Text = Nothing Then
            desctxt.Focus()
        ElseIf TAGCOMBO.SelectedIndex < 0 Then
            TAGCOMBO.Focus()
        ElseIf locationtxt.Text = Nothing Then
            locationtxt.Focus()


        ElseIf stocktxt.Text = Nothing Then
            stocktxt.Focus()

        ElseIf purchasetxt.Text = Nothing Then
            purchasetxt.Focus()
        ElseIf retailtxt.Text = Nothing Then
            retailtxt.Focus()

        ElseIf barcodetxt.Text = Nothing Then
            barcodetxt.Focus()
        Else
            Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
                Try


                    Using cmd As New SQLiteCommand("UPDATE ITEMS SET ItemName = @NameItem,
 Desc = @Description,
 TagID = @IDTag,
 Location = @Loc,
 Stock = @Stock,
 PurchasePrice = @Purchase,
 RetailPrice = @Retail,
 Barcode =  @Barc WHERE SerielNumber = @ID", con)
                        cmd.Parameters.AddWithValue("@NameItem", nametxt.Text)
                        cmd.Parameters.AddWithValue("@Description", desctxt.Text)
                        cmd.Parameters.AddWithValue("@IDTag", TAGCOMBO.SelectedValue)
                        cmd.Parameters.AddWithValue("@Loc", locationtxt.Text)
                        cmd.Parameters.AddWithValue("@Stock", stocktxt.Text)
                        cmd.Parameters.AddWithValue("@Purchase", purchasetxt.Text)
                        cmd.Parameters.AddWithValue("@Retail", retailtxt.Text)
                        cmd.Parameters.AddWithValue("@Barc", barcodetxt.Text)
                        cmd.Parameters.AddWithValue("@ID", ItemID.Text)

                        con.Open()
                        cmd.ExecuteNonQuery()
                        MsgBox("Successfully Updated Item: " & nametxt.Text)

                        con.Close()
                    End Using
                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try
            End Using

        End If
    End Sub
End Class
