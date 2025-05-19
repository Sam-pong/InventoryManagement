
Imports System.Collections.ObjectModel
Imports System.Data

Imports System.ComponentModel

Imports System.Data.SQLite
Imports Xceed.Wpf.Toolkit.PropertyGrid.Attributes
Imports Xceed.Wpf.Toolkit.Primitives
Class TransactionEntry

    Dim inormaybeout As Integer

    Private Sub Button3_click(sender As Object, e As RoutedEventArgs)
        Me.Close()


    End Sub
    Private allItems As ObservableCollection(Of Item)



    Sub LoadItemsFromDB()
        allItems = New ObservableCollection(Of Item)()

        Using con As New SQLiteConnection("Data Source=InvenManage.db")
            con.Open()
            Dim sql As String = "SELECT SerielNumber, ItemName, Desc, Barcode, Location, Stock, TagName, PurchasePrice, RetailPrice, Items.TagID FROM Items INNER JOIN TAGS ON Items.TAGID = TAGS.TAGID"
            Using cmd As New SQLiteCommand(sql, con)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim itm As New Item With {
                            .ID = Convert.ToInt32(reader("SerielNumber")),
                            .Name = reader("ItemName").ToString(),
                            .Description = reader("Desc").ToString,
                                                        .Tag = reader("TagName"),
                            .Barcode = reader("Barcode").ToString,
                            .Location = reader("Location").ToString,
                            .Stock = Convert.ToInt32(reader("Stock")),
                            .BuyingPrice = Convert.ToDecimal(reader("PurchasePrice")),
                            .SellingPrice = Convert.ToDecimal(reader("RetailPrice")),
                            .TagID = Convert.ToDecimal(reader("TagID")),
                            .Quantity = 1
                             }
                        If inorout.SelectedIndex = 1 Then
                            itm.Total = Convert.ToDecimal(reader("RetailPrice"))
                            itm.Type = "Stock Out"
                        Else
                            itm.Total = Convert.ToDecimal(reader("PurchasePrice"))
                            itm.Type = "Stock In"
                        End If

                        allItems.Add(itm)
                    End While
                End Using
            End Using
            con.Close()
        End Using

        SearchDataGrid.ItemsSource = allItems

    End Sub

    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        If e.Key = Key.Escape Then
            If SearchDataGrid.Visibility = Visibility.Visible Then
                SearchDataGrid.Visibility = Visibility.Collapsed
            Else

                Me.Close()
            End If

        End If
    End Sub
    Private Sub searchbar_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles SearchBar.PreviewKeyDown
        If e.Key = Key.Down Then
            SearchDataGrid.Focus()
            SearchDataGrid.SelectedIndex = 0
            Dim row = TryCast(SearchDataGrid.ItemContainerGenerator.ContainerFromIndex(0), DataGridRow)
            If row IsNot Nothing Then row.MoveFocus(New TraversalRequest(FocusNavigationDirection.Next))
            e.Handled = True
        Else
            e.Handled = False

        End If


    End Sub

    Private Sub button5_click(sender As Object, e As RoutedEventArgs)

        Try


            Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)
            If selectedItem Is Nothing Then Exit Sub

            Dim popup As New EditQuanitity()
            popup.UpdatedQuantity = selectedItem.Quantity

            If popup.ShowDialog() = True Then
                selectedItem.Quantity = popup.UpdatedQuantity
                If inormaybeout = 0 Then
                    selectedItem.Total = Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.BuyingPrice)
                    selectedItem.Type = "Stock In"
                ElseIf inormaybeout = 1 Then
                    selectedItem.Total = Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.SellingPrice)
                    selectedItem.Type = "Stock Out"
                End If

                dtgrid.Items.Refresh()

            End If

        Catch ex As Exception

        End Try
    End Sub
    Private Sub delbut_click(sender As Object, e As RoutedEventArgs)
        Dim selectedItem As Item = TryCast(dtgrid.SelectedItem, Item)

        If selectedItem IsNot Nothing Then
            TargetItems.Remove(selectedItem)
            dtgrid.Items.Refresh()
        End If

    End Sub
    Private Sub AddSelectedItem()
        Dim selectedItem = CType(SearchDataGrid.SelectedItem, Item)
        If selectedItem IsNot Nothing Then
            If inormaybeout = 0 Then
                selectedItem.Total = Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.BuyingPrice)
                selectedItem.Type = "Stock In"

            ElseIf inormaybeout = 1 Then
                selectedItem.Total = Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.SellingPrice)
                selectedItem.Type = "Stock Out"
            End If
            Dim alreadyExists As Boolean = TargetItems.Any(Function(i) i.Barcode = selectedItem.Barcode)

            If Not alreadyExists Then
                TargetItems.Add(selectedItem)
                dtgrid.Items.Refresh()
            Else
                MessageBox.Show("Item already added.")
            End If
        End If






    End Sub
    Private Sub thing_keydown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.Key = Key.F2 Then
            button5_click(editquantity, New RoutedEventArgs())
            e.Handled = True
        ElseIf e.Key = Key.Delete Then
            delbut_click(deletebutton, New RoutedEventArgs())
            e.Handled = True

        End If
    End Sub
    Private Sub ItemsGrid_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles SearchDataGrid.PreviewKeyDown
        If e.Key = Key.Enter Then

            AddSelectedItem()

            dtgrid.ItemsSource = Nothing
            dtgrid.ItemsSource = TargetItems

            SearchDataGrid.Visibility = Visibility.Hidden


            With dtgrid
                .Columns(2).Visibility = Visibility.Collapsed
                .Columns(8).Visibility = Visibility.Collapsed
                .Columns(9).Visibility = Visibility.Collapsed

                .Columns(0).Width = 35
                .Columns(1).Width = 210
                .Columns(3).Width = 160
                .Columns(4).Width = 120
                .Columns(5).Width = 100
                .Columns(6).Width = 200
                .Columns(8).Width = 150
                .Columns(10).Width = 150

            End With
            If inorout.SelectedIndex = 0 Then
                dtgrid.Columns(7).Visibility = Visibility.Collapsed
                dtgrid.Columns(6).Visibility = Visibility.Visible
            Else
                dtgrid.Columns(6).Visibility = Visibility.Collapsed
                dtgrid.Columns(7).Visibility = Visibility.Visible
            End If
            e.Handled = True


            SearchBar.Text = Nothing

        End If
    End Sub
    Private ItemsList As ObservableCollection(Of Item)
    Private filteredItems As List(Of Item)
    Private Sub dtgrid_keydown(sender As Object, e As KeyEventArgs) Handles dtgrid.KeyDown
        If e.Key = Key.Enter Then

            SearchDataGrid.Visibility = Visibility.Collapsed
            SearchBar.Focus()
        End If

        If e.Key = Key.Delete Then
            delbut_click(deletebutton, New RoutedEventArgs())
            e.Handled = True

        End If

    End Sub
    Sub testing()

    End Sub
    Private Sub SearchBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles SearchBar.TextChanged
        SearchDataGrid.Visibility = Visibility.Visible
        If allItems Is Nothing Then Return

        Dim search = SearchBar.Text.Replace("'", "''").Trim()


        filteredItems = allItems.Where(Function(item) _
            item.Barcode.ToLower().Contains(search) OrElse
            item.Name.ToLower().Contains(search) OrElse
            item.Tag.ToLower().Contains(search)
        ).ToList()

        SearchDataGrid.ItemsSource = Nothing
        SearchDataGrid.ItemsSource = filteredItems
    End Sub




    Private TargetItems As ObservableCollection(Of Item)

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        TargetItems = New ObservableCollection(Of Item)()
        dtgrid.ItemsSource = TargetItems

        SearchDataGrid.Visibility = Visibility.Collapsed
        LoadItemsFromDB()
        dtpicker.SelectedDate = Today
        SearchBar.Focus()

        ItemsList = New ObservableCollection(Of Item)()
        inorout.SelectedIndex = 0
        inormaybeout = 0


        dtgrid.ItemsSource = ItemsList
        With dtgrid
            .Columns(2).Visibility = Visibility.Collapsed
            .Columns(8).Visibility = Visibility.Collapsed
            .Columns(9).Visibility = Visibility.Collapsed
            .Columns(7).Visibility = Visibility.Collapsed

            .Columns(0).Width = 35
            .Columns(1).Width = 210
            .Columns(3).Width = 160
            .Columns(4).Width = 110
            .Columns(5).Width = 110
            .Columns(6).Width = 200
            .Columns(8).Width = 150
            .Columns(10).Width = 150
        End With


    End Sub

    Private Sub combochangeindex(sender As Object, e As RoutedEventArgs) Handles inorout.SelectionChanged
        Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)
        Try


            If inorout.SelectedIndex = 1 Then
                dtgrid.Columns(6).Visibility = Visibility.Collapsed
                dtgrid.Columns(7).Visibility = Visibility.Visible
                inormaybeout = 1


            Else
                dtgrid.Columns(7).Visibility = Visibility.Collapsed
                dtgrid.Columns(6).Visibility = Visibility.Visible
                inormaybeout = 0

            End If
        Catch ex As Exception

        End Try
    End Sub



End Class
