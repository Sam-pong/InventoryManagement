
Imports System.Collections.ObjectModel
Imports System.Data

Imports System.Data.SQLite
Imports System.Reflection.Emit
Imports System.Windows.Controls.Primitives
Imports Xceed.Wpf.Toolkit.Primitives
Imports Xceed.Wpf.Toolkit.PropertyGrid.Attributes
Class TransactionEntry

    Dim inormaybeout As Integer
    Dim totaloritem As Integer
    Private filteredItems As List(Of Item)
    Private allItems As ObservableCollection(Of Item)
    Private TargetItems As ObservableCollection(Of Item)
    Dim taxes As Decimal
    Dim perctaxes As Decimal
    Dim refnumberexists As Integer
    Private previousSelectionIndex As Integer = -1
    Dim stockcheck As Integer
    Dim currentstock As Integer
    Dim oldref As String
    Dim refexistsbrah As Integer


    Public Sub New()
        InitializeComponent()

        Dim userControl As New Recentsmodule()

        AddHandler userControl.ButtonTriggerRequested, AddressOf UserControl_ButtonTriggerRequested
        AddHandler userControl.LabelChangeRequested, AddressOf ChangeMainLabelText

    End Sub

    Private Sub UserControl_ButtonTriggerRequested()
        refchangeload()
        reflabel.Text = "Updated from UserControl!"
    End Sub

    Private Sub ChangeMainLabelText(ByVal newText As String)
        reflabel.Text = newText
    End Sub



    Private Sub Button3_click(sender As Object, e As RoutedEventArgs)
        Me.Close()


    End Sub 'Close button



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


                        allItems.Add(itm)
                    End While
                End Using
            End Using
            con.Close()
        End Using

        SearchDataGrid.ItemsSource = allItems

    End Sub
    Private Function GetNextTransactionCode(lastCode As String) As String
        Dim prefix As New String(lastCode.TakeWhile(AddressOf Char.IsLetter).ToArray())
        Dim numberPart As New String(lastCode.SkipWhile(AddressOf Char.IsLetter).ToArray())

        Dim number As Integer = Integer.Parse(numberPart)

        If number < 999 Then
            number += 1
            Return prefix & number.ToString("D3")
        Else
            Dim nextPrefix As String = IncrementPrefix(prefix)
            Return nextPrefix & "001"
        End If
    End Function 'cgm (REFNUMBER INCREMENT LOGIC(NUMBER PART))

    Private Function IncrementPrefix(prefix As String) As String
        Dim chars = prefix.ToUpper().ToCharArray().Reverse().ToArray()
        Dim carry = 1
        Dim result As New List(Of Char)

        For i = 0 To chars.Length - 1
            Dim c As Char = chars(i)
            Dim newCharValue = (Asc(c) - Asc("A"c)) + carry

            If newCharValue >= 26 Then
                carry = 1
                result.Add("A"c)
            Else
                carry = 0
                result.Add(Chr(Asc("A"c) + newCharValue))
            End If
        Next

        If carry = 1 Then result.Add("A"c)

        result.Reverse()
        Return New String(result.ToArray())
    End Function 'cgm (REFNUMBER INCREMENT LOGIC (PREFIX PART))


    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        If e.Key = Key.Escape Then
            If SearchDataGrid.Visibility = Visibility.Visible Then
                SearchDataGrid.Visibility = Visibility.Collapsed
            Else

                Me.Close()
            End If

        End If
    End Sub

    Private Sub button8_click(sender As Object, e As RoutedEventArgs)
        Try


            Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)
            If selectedItem Is Nothing Then Exit Sub

            Dim popup As New AddDiscount With {
                .DiscountAdded = selectedItem.Discount
            }


            If popup.ShowDialog() = True Then
                selectedItem.Discount = popup.DiscountAdded
                totaloritem = popup.itemortotal
                If inormaybeout = 0 Then
                    If popup.itemortotal = 0 Then
                        selectedItem.Total = (Convert.ToDecimal(selectedItem.BuyingPrice) - Convert.ToDecimal(selectedItem.Discount)) * Convert.ToDecimal(selectedItem.Quantity)
                        selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.BuyingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total
                        selectedItem.Type = "Stock In"
                    ElseIf popup.itemortotal = 1 Then
                        selectedItem.Total = (Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.BuyingPrice)) - Convert.ToDecimal(selectedItem.Discount)
                        selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.BuyingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total

                        selectedItem.Type = "Stock In"
                    End If

                ElseIf inormaybeout = 1 Then
                    If popup.itemortotal = 0 Then
                        selectedItem.Total = (Convert.ToDecimal(selectedItem.SellingPrice) - Convert.ToDecimal(selectedItem.Discount)) * Convert.ToDecimal(selectedItem.Quantity)
                        selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.SellingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total

                        selectedItem.Type = "Stock Out"
                    ElseIf popup.itemortotal = 1 Then
                        selectedItem.Total = (Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.SellingPrice)) - Convert.ToDecimal(selectedItem.Discount)
                        selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.SellingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total


                        selectedItem.Type = "Stock Out"
                    End If
                End If

                Try
                    Using con As New SQLiteConnection("Data Source=InvenManage.db")
                        Dim sqlquery As String = "UPDATE TRANSACTIONDETAILS SET DISCOUNT = @discount, TOTAL = @total, TOTALDISCOUNT = @totaldiscount WHERE REFNUMBER = @refnumber AND ITEMID = @itemid"
                        Using cmd As New SQLiteCommand(sqlquery, con)
                            con.Open()
                            cmd.Parameters.AddWithValue("@refnumber", reflabel.Text)
                            cmd.Parameters.AddWithValue("@itemid", selectedItem.ID)
                            cmd.Parameters.AddWithValue("@discount", selectedItem.Discount)
                            cmd.Parameters.AddWithValue("@total", selectedItem.Total)
                            cmd.Parameters.AddWithValue("@totaldiscount", selectedItem.TotalDiscount)

                            cmd.ExecuteNonQuery()
                        End Using
                    End Using
                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try
                dtgrid.Items.Refresh()
                subtotalanddiscount()

            End If

        Catch ex As Exception

        End Try
    End Sub ' DISCOUNT WINDOW

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

    Sub updatequantity()
        Try
            Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)

            Using con As New SQLiteConnection("Data Source=InvenManage.db")
                Dim sqlstring As String = "UPDATE TRANSACTIONDETAILS SET QUANTITY = @quantity, TOTAL = @total , TOTALDISCOUNT = @totaldiscount WHERE REFNUMBER = @refnumber and ITEMID = @itemid"
                Using cmd As New SQLiteCommand(sqlstring, con)
                    con.Open()
                    cmd.Parameters.AddWithValue("@refnumber", reflabel.Text)
                    cmd.Parameters.AddWithValue("@itemid", selectedItem.ID)
                    cmd.Parameters.AddWithValue("@quantity", selectedItem.Quantity)
                    cmd.Parameters.AddWithValue("@total", selectedItem.Total)
                    cmd.Parameters.AddWithValue("@totaldiscount", selectedItem.TotalDiscount)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

        Catch ex As SQLiteException
            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try
    End Sub
    Sub quantitymain()
        Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)


        If inormaybeout = 0 Then
            If totaloritem = 0 Then
                selectedItem.Total = (Convert.ToDecimal(selectedItem.BuyingPrice) - Convert.ToDecimal(selectedItem.Discount)) * Convert.ToDecimal(selectedItem.Quantity)
                selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.BuyingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total

                selectedItem.Type = "Stock In"
                updatequantity()
            ElseIf totaloritem = 1 Then
                selectedItem.Total = (Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.BuyingPrice)) - Convert.ToDecimal(selectedItem.Discount)
                selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.BuyingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total

                selectedItem.Type = "Stock In"
                updatequantity()
            End If

        ElseIf inormaybeout = 1 Then
            checkstockdtgrid()

            If stockcheck = 1 Then


                If totaloritem = 0 Then

                    selectedItem.Total = (Convert.ToDecimal(selectedItem.SellingPrice) - Convert.ToDecimal(selectedItem.Discount)) * Convert.ToDecimal(selectedItem.Quantity)
                    selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.SellingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total

                    selectedItem.Type = "Stock Out"
                    updatequantity()
                ElseIf totaloritem = 1 Then
                    selectedItem.Total = (Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.SellingPrice)) - Convert.ToDecimal(selectedItem.Discount)
                    selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.SellingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total

                    selectedItem.Type = "Stock Out"
                    updatequantity()
                End If

            Else

                selectedItem.Quantity = 1


                If totaloritem = 0 Then

                    selectedItem.Total = (Convert.ToDecimal(selectedItem.SellingPrice) - Convert.ToDecimal(selectedItem.Discount)) * Convert.ToDecimal(selectedItem.Quantity)
                    selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.SellingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total

                    selectedItem.Type = "Stock Out"
                    updatequantity()
                ElseIf totaloritem = 1 Then
                    selectedItem.Total = (Convert.ToDecimal(selectedItem.Quantity) * Convert.ToDecimal(selectedItem.SellingPrice)) - Convert.ToDecimal(selectedItem.Discount)
                    selectedItem.TotalDiscount = (Convert.ToDecimal(selectedItem.SellingPrice) * Convert.ToDecimal(selectedItem.Quantity)) - selectedItem.Total

                    selectedItem.Type = "Stock Out"
                    updatequantity()
                End If
            End If

        End If
    End Sub

    Private Sub button5_click(sender As Object, e As RoutedEventArgs)

        Try

            Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)
            If selectedItem Is Nothing Then Exit Sub

            Dim popup As New EditQuanitity With {
                .UpdatedQuantity = selectedItem.Quantity
            }

            If popup.ShowDialog() = True Then
                selectedItem.Quantity = popup.UpdatedQuantity
                quantitymain()


                dtgrid.Items.Refresh()
                subtotalanddiscount()

            End If

        Catch ex As Exception

        End Try
    End Sub 'QUANTITY WINDOW
    Private Sub delbut_click(sender As Object, e As RoutedEventArgs)
        Dim selectedItem As Item = TryCast(dtgrid.SelectedItem, Item)

        If selectedItem IsNot Nothing Then
            TargetItems.Remove(selectedItem)
            Try
                Using con As New SQLiteConnection("Data Source=InvenManage.db")
                    Dim sqlquery As String = "DELETE FROM TRANSACTIONDETAILS WHERE REFNUMBER = @refnumber AND ITEMID = @itemid"
                    Using cmd As New SQLiteCommand(sqlquery, con)
                        con.Open()
                        cmd.Parameters.AddWithValue("@refnumber", reflabel.Text)
                        cmd.Parameters.AddWithValue("@itemid", selectedItem.ID)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As SQLiteException
                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

            Catch ex As Exception
                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

            End Try
            dtgrid.Items.Refresh()
            subtotalanddiscount()
        End If

    End Sub
    Private Sub AddSelectedItem()
        Dim selectedItemsearch = CType(SearchDataGrid.SelectedItem, Item)
        Dim unitprice As Decimal

        If selectedItemsearch IsNot Nothing Then
            If inormaybeout = 0 Then
                selectedItemsearch.Type = "Stock In"

                unitprice = selectedItemsearch.BuyingPrice
                selectedItemsearch.Total = unitprice
            ElseIf inormaybeout = 1 Then
                selectedItemsearch.Type = "Stock Out"
                unitprice = selectedItemsearch.SellingPrice
                selectedItemsearch.Total = unitprice


            End If
            Dim alreadyExists As Boolean = TargetItems.Any(Function(i) i.Barcode = selectedItemsearch.Barcode)

            If Not alreadyExists Then
                TargetItems.Add(selectedItemsearch)

                dtgrid.Items.Refresh()

                Try

                    Using con As New SQLiteConnection("Data Source=InvenManage.db")
                        Dim sqlquery As String = "INSERT INTO TRANSACTIONDETAILS (REFNUMBER, ITEMID, UNITPRICE, QUANTITY, DISCOUNT, TOTAL, TRANSACTIONTYPE,  BARCODE) VALUES (@refnumber, @itemid, @unitprice, @quantity, @discount, @total, @transactiontype, @barcode)"
                        Using cmd As New SQLiteCommand(sqlquery, con)
                            con.Open()
                            cmd.Parameters.AddWithValue("@refnumber", reflabel.Text)
                            cmd.Parameters.AddWithValue("@itemid", selectedItemsearch.ID)
                            cmd.Parameters.AddWithValue("@unitprice", unitprice)
                            cmd.Parameters.AddWithValue("@quantity", selectedItemsearch.Quantity)
                            cmd.Parameters.AddWithValue("@discount", selectedItemsearch.Discount)
                            cmd.Parameters.AddWithValue("@total", selectedItemsearch.Total)
                            cmd.Parameters.AddWithValue("@transactiontype", inormaybeout)
                            cmd.Parameters.AddWithValue("@barcode", selectedItemsearch.Barcode)
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try
                If refnumberexists = 0 Then
                    creatingslot()
                End If

            Else
                Dim selectedBarcode As String = selectedItemsearch.Barcode

                For Each row As Item In dtgrid.Items
                    If row IsNot Nothing AndAlso row.Barcode = selectedBarcode Then
                        dtgrid.SelectedItem = row
                        row.Quantity += 1
                        quantitymain()

                        dtgrid.Items.Refresh()
                        subtotalanddiscount()

                        Exit For
                    End If
                Next


            End If
        End If

    End Sub
    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.Key = Key.F2 Then
            button5_click(editquantity, New RoutedEventArgs())
            e.Handled = True


        End If

        If e.Key = Key.F3 Then
            button8_click(addiscount, New RoutedEventArgs)
            e.Handled = True

        End If
    End Sub

    Private Sub button95_click(sender As Object, e As RoutedEventArgs)
        Dim taxwin As New Taxes
        taxwin.ShowDialog()
    End Sub
    Private Sub ItemsGrid_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles SearchDataGrid.PreviewKeyDown
        If e.Key = Key.Enter Then

            If inormaybeout = 0 Then
                AddSelectedItem()
            ElseIf inormaybeout = 1 Then
                checkstocksearchgrid()



            End If


            dtgrid.ItemsSource = Nothing
            dtgrid.ItemsSource = TargetItems

            SearchDataGrid.Visibility = Visibility.Collapsed


            With dtgrid
                .Columns(2).Visibility = Visibility.Collapsed
                .Columns(8).Visibility = Visibility.Collapsed
                .Columns(9).Visibility = Visibility.Collapsed
                .Columns(14).Visibility = Visibility.Collapsed


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
                inormaybeout = 0
            Else
                dtgrid.Columns(6).Visibility = Visibility.Collapsed
                dtgrid.Columns(7).Visibility = Visibility.Visible
                inormaybeout = 1

            End If
            e.Handled = True

            SearchBar.Text = Nothing
            subtotalanddiscount()

        End If
    End Sub

    Private Sub dtgrid_keydown(sender As Object, e As KeyEventArgs) Handles dtgrid.KeyDown
        If e.Key = Key.Enter Then

            SearchDataGrid.Visibility = Visibility.Collapsed
            SearchBar.Focus()
        End If


    End Sub
    Private Sub dtgrid_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles dtgrid.PreviewKeyDown
        If e.Key = Key.Delete Then
            delbut_click(deletebutton, New RoutedEventArgs())
            e.Handled = True
        End If
    End Sub
    Sub refcodeload()
        Try
            Using con As New SQLiteConnection("Data Source=InvenManage.db")
                Using cmd As New SQLiteCommand("SELECT MAX(REFNUMBER) FROM TRANSACTIONS", con)
                    con.Open()
                    Using reader As SQLiteDataReader = cmd.ExecuteReader
                        With reader.Read

                            If Not reader.IsDBNull(reader.GetOrdinal("MAX(REFNUMBER)")) Then
                                reflabel.Text = GetNextTransactionCode(reader("MAX(REFNUMBER)"))
                                oldref = reflabel.Text

                            Else
                                reflabel.Text = "A001"
                                oldref = reflabel.Text

                            End If
                        End With
                    End Using
                End Using

            End Using

        Catch ex As SQLiteException
            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try
    End Sub

    Sub taxload()
        Try
            taxes = 0
            perctaxes = 0
            Using con As New SQLiteConnection("Data Source=InvenManage.db")
                Using cmd As New SQLiteCommand("SELECT SUM(TAX) FROM TAXES WHERE APPLYBYDEFAULT = 1 AND PERCORAMOUNT = 0", con)
                    con.Open()

                    Using reader As SQLiteDataReader = cmd.ExecuteReader

                        With reader.Read
                            If Not reader.IsDBNull(reader.GetOrdinal("SUM(TAX)")) Then
                                taxes = Convert.ToDecimal(reader("SUM(TAX)"))
                            Else
                                taxes = 0
                            End If
                        End With

                    End Using
                End Using
                Using cmd2 As New SQLiteCommand("SELECT SUM(TAX) FROM TAXES WHERE APPLYBYDEFAULT = 1 AND PERCORAMOUNT = 1", con)

                    Using reader2 As SQLiteDataReader = cmd2.ExecuteReader
                        With reader2.Read
                            If Not reader2.IsDBNull(reader2.GetOrdinal("SUM(TAX)")) = True Then

                                perctaxes = Convert.ToDecimal(reader2("SUM(TAX)"))
                            End If
                        End With
                    End Using
                End Using
            End Using


        Catch ex As SQLiteException
            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try
    End Sub
    Private Sub Window_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.PreviewMouseDown
        Dim clickedElement = TryCast(Mouse.DirectlyOver, DependencyObject)

        If Not IsDescendant(SearchBar, clickedElement) AndAlso Not IsDescendant(SearchDataGrid, clickedElement) Then
            SearchDataGrid.Visibility = Visibility.Collapsed
            If refexistsbrah = 0 Then
                reflabel.Text = oldref


            End If

        End If
    End Sub  ' got from the internet

    Private Function IsDescendant(parent As DependencyObject, child As DependencyObject) As Boolean
        While child IsNot Nothing
            If child Is parent Then
                Return True
            End If
            child = VisualTreeHelper.GetParent(child)
        End While
        Return False
    End Function  ' got from the internet

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


    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        loadwindow()
                checkdrafts()
    End Sub



    Sub loadwindow()
        refcodeload()

        TargetItems = New ObservableCollection(Of Item)()
        dtgrid.ItemsSource = TargetItems

        SearchDataGrid.Visibility = Visibility.Collapsed
        LoadItemsFromDB()
        dtpicker.SelectedDate = Today
        SearchBar.Focus()
        previousSelectionIndex = 0

        inorout.SelectedIndex = 0
        inormaybeout = 0
        grandtotalam.Content = 0
        discounttotal.Content = 0
        subtotal.Content = 0
        taxs.Content = 0
        customerlbl_.Text = " StockIN"
        refnumberexists = 0

        With dtgrid
            .Columns(2).Visibility = Visibility.Collapsed
            .Columns(8).Visibility = Visibility.Collapsed
            .Columns(9).Visibility = Visibility.Collapsed
            .Columns(14).Visibility = Visibility.Collapsed

            .Columns(0).Width = 35
            .Columns(1).Width = 210
            .Columns(3).Width = 160
            .Columns(4).Width = 110
            .Columns(5).Width = 110
            .Columns(6).Width = 200
            .Columns(8).Width = 150
            .Columns(10).Width = 150
        End With


        taxload()

    End Sub


    Sub checkdrafts()
        Using con As New SQLiteConnection("Data Source=InvenManage.db")
            Dim sql As String = "SELECT COUNT(*) FROM TRANSACTIONS WHERE STATUS = 'Draft' AND SN = (SELECT MAX(SN) FROM TRANSACTIONS)"
            Using cmd As New SQLiteCommand(sql, con)
                con.Open()
                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                If count = 0 Then
                    Return
                ElseIf count = 1 Then
                    MsgBox("You have a draft transaction. Please complete it before creating a new one.", MsgBoxStyle.Exclamation, "Draft Transaction")
                    loadlastdraft()
                End If
            End Using
        End Using
    End Sub

    Sub loadlastdraft()
        Try


            Using con As New SQLiteConnection("Data Source=InvenManage.db")
                Dim sql As String = "SELECT 
    td.REFNUMBER, td.ITEMID, i.ITEMNAME, i.STOCK, i.TAGID, 
    i.PurchasePrice, i.RetailPrice, td.QUANTITY, td.DISCOUNT, 
    td.TOTAL, td.TOTALDISCOUNT, td.TRANSACTIONTYPE, i.BARCODE, i.Location, i.Desc, tgs.TAGNAME, CASE WHEN td.TRANSACTIONTYPE = 0 THEN 'Stock In' ELSE 'Stock Out' END AS TYPENAME
FROM TRANSACTIONDETAILS td
JOIN ITEMS i ON td.ITEMID = i.SerielNumber 
JOIN TAGS tgs ON i.TAGID = tgs.TAGID

WHERE td.REFNUMBER = (
    SELECT MAX(REFNUMBER) FROM TRANSACTIONS
);
"
                Using cmd As New SQLiteCommand(sql, con)
                    con.Open()
                    Try
                        Using reader As SQLiteDataReader = cmd.ExecuteReader
                            While reader.Read()
                                reflabel.Text = reader("REFNUMBER").ToString()
                                oldref = reflabel.Text

                                Dim itm As New Item With {
                                    .ID = reader("ITEMID"),
                                    .Name = reader("ITEMNAME").ToString(),
                                    .Description = If(IsDBNull(reader("Desc")), "", reader("Desc").ToString()),
                                    .TagID = Convert.ToInt32(reader("TAGID")),
                                    .Tag = reader("TAGNAME"),
                                    .Barcode = reader("BARCODE"),
                                    .Location = If(IsDBNull(reader("Location")), "", reader("Location").ToString()),
                                    .Stock = Convert.ToInt32(reader("STOCK")),
                                    .BuyingPrice = Convert.ToDecimal(reader("PurchasePrice")),
                                    .SellingPrice = Convert.ToDecimal(reader("RetailPrice")),
                                    .Quantity = Convert.ToInt32(reader("QUANTITY")),
                                    .Discount = If(IsDBNull(reader("DISCOUNT")), 0D, Convert.ToDecimal(reader("DISCOUNT"))),
                                    .Total = Convert.ToDecimal(reader("TOTAL")),
                                    .TotalDiscount = If(IsDBNull(reader("TOTALDISCOUNT")), 0D, Convert.ToDecimal(reader("TOTALDISCOUNT"))),
                                    .Type = reader("TYPENAME")
                                }

                                If itm.Type = "Stock In" Then
                                    inorout.SelectedIndex = 0
                                ElseIf itm.Type = "Stock Out" Then
                                    inorout.SelectedIndex = 1
                                End If
                                refnumberexists = 1

                                TargetItems.Add(itm)
                            End While
                            dtgrid.ItemsSource = TargetItems
                            With dtgrid
                                .Columns(2).Visibility = Visibility.Collapsed
                                .Columns(8).Visibility = Visibility.Collapsed
                                .Columns(9).Visibility = Visibility.Collapsed
                                .Columns(14).Visibility = Visibility.Collapsed

                                .Columns(0).Width = 35
                                .Columns(1).Width = 210
                                .Columns(3).Width = 160
                                .Columns(4).Width = 110
                                .Columns(5).Width = 110
                                .Columns(6).Width = 200
                                .Columns(8).Width = 150
                                .Columns(10).Width = 150
                            End With

                        End Using
                        subtotalanddiscount()

                    Catch ex As SQLiteException
                        MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                    Catch ex As Exception
                        MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                    End Try
                End Using

            End Using
        Catch ex As SQLiteException
            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try

    End Sub

    Sub checkrefnumber()
        Try
            Using con As New SQLiteConnection("Data Source=InvenManage.db")
                Dim sqlstring As String = "SELECT REFNUMBER FROM TRANSACTIONS WHERE REFNUMBER = @refnumber"
                Using cmd As New SQLiteCommand(sqlstring, con)
                    cmd.Parameters.AddWithValue("@refnumber", reflabel.Text)
                    con.Open()
                    Using reader As SQLiteDataReader = cmd.ExecuteReader
                        If Not reader.HasRows Then
                            refnumberexists = 0
                        Else
                            refnumberexists = 1

                        End If
                    End Using
                End Using
            End Using
        Catch ex As SQLiteException
            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try
    End Sub
    Sub creatingslot()
        Try
            Using con As New SQLiteConnection("Data Source=InvenManage.db")
                con.Open()
                Dim sqlstring2 As String = "INSERT INTO TRANSACTIONS (REFNUMBER, DATE, CUSTOMER , TOTALAMOUNT, SUBTOTAL, DISCOUNTTOTAL, TAXAMOUNT, TRANSTYPE, NOTES, STATUS) VALUES (@refnumber, @date, @customer, @totalamount, @subtotal,  @discounttotal, @taxamount, @transtype, @notes, @status)"
                Using cmd2 As New SQLiteCommand(sqlstring2, con)
                    cmd2.Parameters.AddWithValue("@refnumber", reflabel.Text)
                    cmd2.Parameters.AddWithValue("@date", dtpicker.SelectedDate)
                    cmd2.Parameters.AddWithValue("@customer", "testing")
                    cmd2.Parameters.AddWithValue("@totalamount", 0)
                    cmd2.Parameters.AddWithValue("@subtotal", 0)
                    cmd2.Parameters.AddWithValue("@discounttotal", 0)
                    cmd2.Parameters.AddWithValue("@taxamount", 0)
                    cmd2.Parameters.AddWithValue("@transtype", 0)
                    cmd2.Parameters.AddWithValue("@notes", 0)
                    cmd2.Parameters.AddWithValue("@status", "Draft")

                    cmd2.ExecuteNonQuery()
                    refnumberexists = 1
                End Using
            End Using


        Catch ex As SQLiteException
            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try

    End Sub


    Private Sub combochangeindex(sender As Object, e As RoutedEventArgs) Handles inorout.SelectionChanged
        Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)
        If dtgrid.Items.Count = 0 Then
            If inorout.SelectedIndex = 1 Then
                dtgrid.Columns(6).Visibility = Visibility.Collapsed
                dtgrid.Columns(7).Visibility = Visibility.Visible
                inormaybeout = 1
                customerlbl_.Text = Nothing
            Else
                dtgrid.Columns(7).Visibility = Visibility.Collapsed
                dtgrid.Columns(6).Visibility = Visibility.Visible
                inormaybeout = 0
                customerlbl_.Text = " StockIN"
            End If
            previousSelectionIndex = inorout.SelectedIndex
        Else
            MsgBox("You cannot change the transaction type after adding items.", MsgBoxStyle.Exclamation, "Error")
            inorout.SelectedIndex = previousSelectionIndex
            Return
        End If
    End Sub



    Sub subtotalanddiscount()
        Dim totalDiscount As Decimal = 0
        Dim totalSubtotal As Decimal = 0

        For Each item As Item In dtgrid.Items
            If item IsNot Nothing Then
                totalDiscount += Convert.ToDecimal(item.TotalDiscount)
                totalSubtotal += Convert.ToDecimal(item.Total)
            End If
        Next

        discounttotal.Content = totalDiscount.ToString("N2")
        subtotal.Content = totalSubtotal.ToString("N2")
        taxload()

        Dim grandtotal As Decimal
        If taxcheck.IsChecked = True Then
            Dim taxesrealthistime As Decimal = taxes / 100
            grandtotal = totalSubtotal + (totalSubtotal * taxesrealthistime) + perctaxes
            grandtotalam.Content = Convert.ToDecimal(grandtotal)
            taxs.Content = grandtotal - totalSubtotal
        ElseIf taxcheck.IsChecked = False Then
            taxs.Content = 0
            grandtotalam.Content = totalSubtotal
        End If



    End Sub

    Private Sub taxcheck_check(sender As Object, e As RoutedEventArgs) Handles taxcheck.Checked
        If taxcheck.IsChecked = True Then
            taxlabel.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF000000"))

            taxs.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF000000"))
            subtotalanddiscount()
            taxload()

        End If
    End Sub


    Private Sub taxcheck_uncheck(sender As Object, e As RoutedEventArgs) Handles taxcheck.Unchecked
        If taxcheck.IsChecked = False Then
            taxlabel.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF5A5353"))
            taxs.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF5A5353"))
            subtotalanddiscount()
            taxs.Content = 0
        End If

    End Sub

    Private Sub savebutton_click()
        If dtgrid.Items.Count = 0 Then
            MsgBox("No items to save", MsgBoxStyle.Exclamation, "Error")
            Return
        Else

            Try
                Using con As New SQLiteConnection("Data Source=InvenManage.db")
                    Dim sqlquery As String = "UPDATE TRANSACTIONS SET STATUS = 'Completed', TOTALAMOUNT = @totalamount, SUBTOTAL = @subtotal, DISCOUNTTOTAL = @discounttotal, TAXAMOUNT = @taxamount, TRANSTYPE = @transtype, NOTES = @notes, CUSTOMER = @customer   WHERE REFNUMBER = @refnumber"
                    Using cmd As New SQLiteCommand(sqlquery, con)
                        con.Open()
                        cmd.Parameters.AddWithValue("@refnumber", reflabel.Text)
                        cmd.Parameters.AddWithValue("@totalamount", grandtotalam.Content)
                        cmd.Parameters.AddWithValue("@subtotal", subtotal.Content)
                        cmd.Parameters.AddWithValue("@discounttotal", discounttotal.Content)
                        cmd.Parameters.AddWithValue("@taxamount", taxs.Content)
                        cmd.Parameters.AddWithValue("@customer", customerlbl_.Text)
                        cmd.Parameters.AddWithValue("@transtype", inormaybeout)
                        cmd.Parameters.AddWithValue("@notes", notes.Text)
                        cmd.ExecuteNonQuery()
                        stockupdate()
                        loadwindow()

                        MsgBox("Saved successfully", MsgBoxStyle.Information, "Success")
                    End Using
                End Using
            Catch ex As SQLiteException
                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

            Catch ex As Exception
                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

            End Try
        End If

    End Sub


    Private Sub loadlasttransaction_click(sender As Object, e As RoutedEventArgs)
        If dtgrid.Items.Count = 0 Then

            Try


                Using con As New SQLiteConnection("Data Source=InvenManage.db")
                    Dim sql As String = "SELECT 
    td.REFNUMBER, td.ITEMID, i.ITEMNAME, i.STOCK, i.TAGID, 
    i.PurchasePrice, i.RetailPrice, td.QUANTITY, td.DISCOUNT, 
    td.TOTAL, td.TOTALDISCOUNT, td.TRANSACTIONTYPE, i.BARCODE, i.Location, i.Desc, tgs.TAGNAME, CASE WHEN td.TRANSACTIONTYPE = 0 THEN 'Stock In' ELSE 'Stock Out' END AS TYPENAME
FROM TRANSACTIONDETAILS td
JOIN ITEMS i ON td.ITEMID = i.SerielNumber 
JOIN TAGS tgs ON i.TAGID = tgs.TAGID

WHERE td.REFNUMBER = (
    SELECT MAX(REFNUMBER) FROM TRANSACTIONS
);
"
                    Using cmd As New SQLiteCommand(sql, con)
                        con.Open()
                        Try
                            Using reader As SQLiteDataReader = cmd.ExecuteReader
                                While reader.Read()
                                    reflabel.Text = reader("REFNUMBER").ToString()

                                    Dim itm As New Item With {
                                        .ID = reader("ITEMID"),
                                        .Name = reader("ITEMNAME").ToString(),
                                        .Description = If(IsDBNull(reader("Desc")), "", reader("Desc").ToString()),
                                        .TagID = Convert.ToInt32(reader("TAGID")),
                                        .Tag = reader("TAGNAME"),
                                        .Barcode = reader("BARCODE"),
                                        .Location = If(IsDBNull(reader("Location")), "", reader("Location").ToString()),
                                        .Stock = Convert.ToInt32(reader("STOCK")),
                                        .BuyingPrice = Convert.ToDecimal(reader("PurchasePrice")),
                                        .SellingPrice = Convert.ToDecimal(reader("RetailPrice")),
                                        .Quantity = Convert.ToInt32(reader("QUANTITY")),
                                        .Discount = If(IsDBNull(reader("DISCOUNT")), 0D, Convert.ToDecimal(reader("DISCOUNT"))),
                                        .Total = Convert.ToDecimal(reader("TOTAL")),
                                        .TotalDiscount = If(IsDBNull(reader("TOTALDISCOUNT")), 0D, Convert.ToDecimal(reader("TOTALDISCOUNT"))),
                                        .Type = reader("TYPENAME")
                                    }

                                    If itm.Type = "Stock In" Then
                                        inorout.SelectedIndex = 0
                                    ElseIf itm.Type = "Stock Out" Then
                                        inorout.SelectedIndex = 1
                                    End If


                                    TargetItems.Add(itm)
                                End While
                                dtgrid.ItemsSource = TargetItems
                                With dtgrid
                                    .Columns(2).Visibility = Visibility.Collapsed
                                    .Columns(8).Visibility = Visibility.Collapsed
                                    .Columns(9).Visibility = Visibility.Collapsed
                                    .Columns(7).Visibility = Visibility.Collapsed
                                    .Columns(14).Visibility = Visibility.Collapsed

                                    .Columns(0).Width = 35
                                    .Columns(1).Width = 210
                                    .Columns(3).Width = 160
                                    .Columns(4).Width = 110
                                    .Columns(5).Width = 110
                                    .Columns(6).Width = 200
                                    .Columns(8).Width = 150
                                    .Columns(10).Width = 150
                                End With

                            End Using
                            subtotalanddiscount()

                        Catch ex As SQLiteException
                            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                        Catch ex As Exception
                            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                        End Try
                    End Using

                End Using
            Catch ex As SQLiteException
                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

            Catch ex As Exception
                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

            End Try
        Else
            MsgBox("Discard current entry?", MsgBoxStyle.OkCancel, "Error")
            If MsgBoxResult.Ok Then
                Try
                    Using con As New SQLiteConnection("Data Source=InvenManage.db")
                        con.Open()

                        TargetItems.Clear()
                        dtgrid.Items.Refresh()

                        Try


                            Dim sql As String = "SELECT 
    td.REFNUMBER, td.ITEMID, i.ITEMNAME, i.STOCK, i.TAGID, 
    i.PurchasePrice, i.RetailPrice, td.QUANTITY, td.DISCOUNT, 
    td.TOTAL, td.TOTALDISCOUNT, td.TRANSACTIONTYPE, i.BARCODE, i.Location, i.Desc, tgs.TAGNAME, CASE WHEN td.TRANSACTIONTYPE = 0 THEN 'Stock In' ELSE 'Stock Out' END AS TYPENAME
FROM TRANSACTIONDETAILS td
JOIN ITEMS i ON td.ITEMID = i.SerielNumber 
JOIN TAGS tgs ON i.TAGID = tgs.TAGID

WHERE td.REFNUMBER = (
    SELECT REFNUMBER FROM TRANSACTIONS
ORDER BY REFNUMBER DESC
LIMIT 1 OFFSET 1
);
"
                            Using cmd2 As New SQLiteCommand(sql, con)
                                Try
                                    Using reader As SQLiteDataReader = cmd2.ExecuteReader
                                        While reader.Read()
                                            reflabel.Text = reader("REFNUMBER").ToString()
                                            Dim itm As New Item With {
                                                    .ID = reader("ITEMID"),
                                                    .Name = reader("ITEMNAME").ToString(),
                                                    .Description = If(IsDBNull(reader("Desc")), "", reader("Desc").ToString()),
                                                    .TagID = Convert.ToInt32(reader("TAGID")),
                                                    .Tag = reader("TAGNAME"),
                                                    .Barcode = reader("BARCODE"),
                                                    .Location = If(IsDBNull(reader("Location")), "", reader("Location").ToString()),
                                                    .Stock = Convert.ToInt32(reader("STOCK")),
                                                    .BuyingPrice = Convert.ToDecimal(reader("PurchasePrice")),
                                                    .SellingPrice = Convert.ToDecimal(reader("RetailPrice")),
                                                    .Quantity = Convert.ToInt32(reader("QUANTITY")),
                                                    .Discount = If(IsDBNull(reader("DISCOUNT")), 0D, Convert.ToDecimal(reader("DISCOUNT"))),
                                                    .Total = Convert.ToDecimal(reader("TOTAL")),
                                                    .TotalDiscount = If(IsDBNull(reader("TOTALDISCOUNT")), 0D, Convert.ToDecimal(reader("TOTALDISCOUNT"))),
                                                    .Type = reader("TYPENAME")
                                                }

                                            If itm.Type = "Stock In" Then
                                                inorout.SelectedIndex = 0
                                            ElseIf itm.Type = "Stock Out" Then
                                                inorout.SelectedIndex = 1
                                            End If


                                            TargetItems.Add(itm)
                                        End While
                                        dtgrid.ItemsSource = TargetItems
                                        With dtgrid
                                            .Columns(2).Visibility = Visibility.Collapsed
                                            .Columns(8).Visibility = Visibility.Collapsed
                                            .Columns(9).Visibility = Visibility.Collapsed
                                            .Columns(7).Visibility = Visibility.Collapsed
                                            .Columns(14).Visibility = Visibility.Collapsed

                                            .Columns(0).Width = 35
                                            .Columns(1).Width = 210
                                            .Columns(3).Width = 160
                                            .Columns(4).Width = 110
                                            .Columns(5).Width = 110
                                            .Columns(6).Width = 200
                                            .Columns(8).Width = 150
                                            .Columns(10).Width = 150
                                        End With

                                    End Using
                                    subtotalanddiscount()
                                Catch ex As SQLiteException
                                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                                Catch ex As Exception
                                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")
                                End Try

                            End Using


                        Catch ex As SQLiteException
                            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                        Catch ex As Exception
                            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                        End Try
                    End Using


                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try


            ElseIf MsgBoxResult.Cancel Then
                Return
            End If
        End If

    End Sub


    Private Sub newbutton_click()
        If dtgrid.Items.Count = 0 Then
            loadwindow()
        Else
            MsgBox("Discard current entry?", MsgBoxStyle.OkCancel, "Error")
            If MsgBoxResult.Ok Then
                loadwindow()
            ElseIf MsgBoxResult.Cancel Then
                Return
            End If
        End If
    End Sub




    Function GetStockFromdtgrid()
        Using con As New SQLiteConnection("Data Source=InvenManage.db")
            Dim sqlquery As String = "SELECT STOCK FROM ITEMS WHERE SERIELNUMBER = @itemid"
            Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)
            Using cmd As New SQLiteCommand(sqlquery, con)
                cmd.Parameters.AddWithValue("itemid", selectedItem.ID)
                con.Open()
                Return Convert.ToInt32(cmd.ExecuteScalar)
                currentstock = Convert.ToInt32(cmd.ExecuteScalar)
            End Using
        End Using
    End Function
    Function GetStockFromsearchgrid()
        Using con As New SQLiteConnection("Data Source=InvenManage.db")
            Dim sqlquery As String = "SELECT STOCK FROM ITEMS WHERE SERIELNUMBER = @itemid"
            Dim selectedItem = TryCast(SearchDataGrid.SelectedItem, Item)
            Using cmd As New SQLiteCommand(sqlquery, con)
                cmd.Parameters.AddWithValue("itemid", selectedItem.ID)
                con.Open()
                Return Convert.ToInt32(cmd.ExecuteScalar)
            End Using
        End Using
    End Function
    Sub checkstockdtgrid()
        Dim selectedItem = TryCast(dtgrid.SelectedItem, Item)
        currentstock = GetStockFromdtgrid()
        If selectedItem.Quantity > currentstock Then
            MessageBox.Show("Not enough stock available.")
            stockcheck = 0
            Exit Sub
        Else
            stockcheck = 1
        End If
    End Sub
    Sub checkstocksearchgrid()
        Dim selectedItem = TryCast(SearchDataGrid.SelectedItem, Item)
        currentstock = GetStockFromsearchgrid()
        If selectedItem.Quantity > currentstock Then
            MessageBox.Show("Not enough stock available.")
            stockcheck = 0
            Exit Sub
        Else
            stockcheck = 1
            AddSelectedItem()
        End If
    End Sub


    Sub stockupdate()
        Using con As New SQLiteConnection("Data Source=InvenManage.db")
            con.Open()
            For Each item As Item In dtgrid.Items
                If item IsNot Nothing Then
                    Dim stockChange As Integer = item.Quantity
                    Dim sql As String = ""
                    If item.Type = "Stock In" Then
                        sql = "UPDATE ITEMS SET STOCK = STOCK + @qty WHERE SerielNumber = @id"
                    ElseIf item.Type = "Stock Out" Then
                        sql = "UPDATE ITEMS SET STOCK = STOCK - @qty WHERE SerielNumber = @id"
                    End If
                    Using cmd As New SQLiteCommand(sql, con)
                        cmd.Parameters.AddWithValue("@qty", stockChange)
                        cmd.Parameters.AddWithValue("@id", item.ID)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            Next
        End Using
    End Sub


    Private Sub reftxtchange(sender As Object, e As KeyEventArgs) Handles reflabel.PreviewKeyDown
        If e.Key = Key.Enter Then
            refchangeload()
        End If


    End Sub

    Sub refchangeload()
        Try
            Using con As New SQLiteConnection("Data Source=InvenManage.db")
                Dim sqlquery As String = "SELECT COUNT(*) FROM TRANSACTIONS WHERE REFNUMBER = @refnumber"
                Using cmdmain As New SQLiteCommand(sqlquery, con)
                    cmdmain.Parameters.AddWithValue("@refnumber", reflabel.Text)
                    con.Open()
                    Using readermain As SQLiteDataReader = cmdmain.ExecuteReader
                        If readermain.Read() Then
                            Dim count As Integer = Convert.ToInt32(readermain(0))
                            If count > 0 Then
                                refexistsbrah = 1
                                If dtgrid.Items.Count = 0 Then

                                    Try


                                        Dim sql As String = "SELECT 
    td.REFNUMBER, td.ITEMID, i.ITEMNAME, i.STOCK, i.TAGID, 
    i.PurchasePrice, i.RetailPrice, td.QUANTITY, td.DISCOUNT, 
    td.TOTAL, td.TOTALDISCOUNT, td.TRANSACTIONTYPE, i.BARCODE, i.Location, i.Desc, tgs.TAGNAME, CASE WHEN td.TRANSACTIONTYPE = 0 THEN 'Stock In' ELSE 'Stock Out' END AS TYPENAME
FROM TRANSACTIONDETAILS td
JOIN ITEMS i ON td.ITEMID = i.SerielNumber 
JOIN TAGS tgs ON i.TAGID = tgs.TAGID

WHERE td.REFNUMBER = @refnumber

"
                                        Using cmd As New SQLiteCommand(sql, con)
                                            cmd.Parameters.AddWithValue("@refnumber", reflabel.Text)
                                            Try
                                                Using reader As SQLiteDataReader = cmd.ExecuteReader
                                                    While reader.Read()
                                                        reflabel.Text = reader("REFNUMBER").ToString()

                                                        Dim itm As New Item With {
                                    .ID = reader("ITEMID"),
                                    .Name = reader("ITEMNAME").ToString(),
                                    .Description = If(IsDBNull(reader("Desc")), "", reader("Desc").ToString()),
                                    .TagID = Convert.ToInt32(reader("TAGID")),
                                    .Tag = reader("TAGNAME"),
                                    .Barcode = reader("BARCODE"),
                                    .Location = If(IsDBNull(reader("Location")), "", reader("Location").ToString()),
                                    .Stock = Convert.ToInt32(reader("STOCK")),
                                    .BuyingPrice = Convert.ToDecimal(reader("PurchasePrice")),
                                    .SellingPrice = Convert.ToDecimal(reader("RetailPrice")),
                                    .Quantity = Convert.ToInt32(reader("QUANTITY")),
                                    .Discount = If(IsDBNull(reader("DISCOUNT")), 0D, Convert.ToDecimal(reader("DISCOUNT"))),
                                    .Total = Convert.ToDecimal(reader("TOTAL")),
                                    .TotalDiscount = If(IsDBNull(reader("TOTALDISCOUNT")), 0D, Convert.ToDecimal(reader("TOTALDISCOUNT"))),
                                    .Type = reader("TYPENAME")
                                }

                                                        If itm.Type = "Stock In" Then
                                                            inorout.SelectedIndex = 0
                                                        ElseIf itm.Type = "Stock Out" Then
                                                            inorout.SelectedIndex = 1
                                                        End If


                                                        TargetItems.Add(itm)
                                                    End While
                                                    dtgrid.ItemsSource = TargetItems
                                                    With dtgrid
                                                        .Columns(2).Visibility = Visibility.Collapsed
                                                        .Columns(8).Visibility = Visibility.Collapsed
                                                        .Columns(9).Visibility = Visibility.Collapsed
                                                        .Columns(7).Visibility = Visibility.Collapsed
                                                        .Columns(14).Visibility = Visibility.Collapsed

                                                        .Columns(0).Width = 35
                                                        .Columns(1).Width = 210
                                                        .Columns(3).Width = 160
                                                        .Columns(4).Width = 110
                                                        .Columns(5).Width = 110
                                                        .Columns(6).Width = 200
                                                        .Columns(8).Width = 150
                                                        .Columns(10).Width = 150
                                                    End With

                                                End Using
                                                subtotalanddiscount()

                                            Catch ex As SQLiteException
                                                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                                            Catch ex As Exception
                                                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                                            End Try
                                        End Using


                                    Catch ex As SQLiteException
                                        MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                                    Catch ex As Exception
                                        MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                                    End Try
                                Else
                                    MsgBox("Discard current entry?", MsgBoxStyle.OkCancel, "Error")
                                    If MsgBoxResult.Ok Then
                                        Try

                                            TargetItems.Clear()
                                            dtgrid.Items.Refresh()

                                            Try


                                                Dim sql As String = "SELECT 
    td.REFNUMBER, td.ITEMID, i.ITEMNAME, i.STOCK, i.TAGID, 
    i.PurchasePrice, i.RetailPrice, td.QUANTITY, td.DISCOUNT, 
    td.TOTAL, td.TOTALDISCOUNT, td.TRANSACTIONTYPE, i.BARCODE, i.Location, i.Desc, tgs.TAGNAME, CASE WHEN td.TRANSACTIONTYPE = 0 THEN 'Stock In' ELSE 'Stock Out' END AS TYPENAME
FROM TRANSACTIONDETAILS td
JOIN ITEMS i ON td.ITEMID = i.SerielNumber 
JOIN TAGS tgs ON i.TAGID = tgs.TAGID

WHERE td.REFNUMBER = @refnumber
"
                                                Using cmd2 As New SQLiteCommand(sql, con)
                                                    cmd2.Parameters.AddWithValue("@refnumber", reflabel.Text)
                                                    Try
                                                        Using reader As SQLiteDataReader = cmd2.ExecuteReader
                                                            While reader.Read()
                                                                reflabel.Text = reader("REFNUMBER").ToString()
                                                                Dim itm As New Item With {
                                            .ID = reader("ITEMID"),
                                            .Name = reader("ITEMNAME").ToString(),
                                            .Description = If(IsDBNull(reader("Desc")), "", reader("Desc").ToString()),
                                            .TagID = Convert.ToInt32(reader("TAGID")),
                                            .Tag = reader("TAGNAME"),
                                            .Barcode = reader("BARCODE"),
                                            .Location = If(IsDBNull(reader("Location")), "", reader("Location").ToString()),
                                            .Stock = Convert.ToInt32(reader("STOCK")),
                                            .BuyingPrice = Convert.ToDecimal(reader("PurchasePrice")),
                                            .SellingPrice = Convert.ToDecimal(reader("RetailPrice")),
                                            .Quantity = Convert.ToInt32(reader("QUANTITY")),
                                            .Discount = If(IsDBNull(reader("DISCOUNT")), 0D, Convert.ToDecimal(reader("DISCOUNT"))),
                                            .Total = Convert.ToDecimal(reader("TOTAL")),
                                            .TotalDiscount = If(IsDBNull(reader("TOTALDISCOUNT")), 0D, Convert.ToDecimal(reader("TOTALDISCOUNT"))),
                                            .Type = reader("TYPENAME")
                                        }

                                                                If itm.Type = "Stock In" Then
                                                                    inorout.SelectedIndex = 0
                                                                ElseIf itm.Type = "Stock Out" Then
                                                                    inorout.SelectedIndex = 1
                                                                End If


                                                                TargetItems.Add(itm)
                                                            End While
                                                            dtgrid.ItemsSource = TargetItems
                                                            With dtgrid
                                                                .Columns(2).Visibility = Visibility.Collapsed
                                                                .Columns(8).Visibility = Visibility.Collapsed
                                                                .Columns(9).Visibility = Visibility.Collapsed
                                                                .Columns(7).Visibility = Visibility.Collapsed
                                                                .Columns(14).Visibility = Visibility.Collapsed

                                                                .Columns(0).Width = 35
                                                                .Columns(1).Width = 210
                                                                .Columns(3).Width = 160
                                                                .Columns(4).Width = 110
                                                                .Columns(5).Width = 110
                                                                .Columns(6).Width = 200
                                                                .Columns(8).Width = 150
                                                                .Columns(10).Width = 150
                                                            End With

                                                        End Using
                                                        subtotalanddiscount()
                                                    Catch ex As SQLiteException
                                                        MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                                                    Catch ex As Exception
                                                        MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")
                                                    End Try

                                                End Using


                                            Catch ex As SQLiteException
                                                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                                            Catch ex As Exception
                                                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                                            End Try


                                        Catch ex As SQLiteException
                                            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                                        Catch ex As Exception
                                            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                                        End Try


                                    ElseIf MsgBoxResult.Cancel Then
                                        Return
                                    End If
                                End If

                                Return
                            Else
                                reflabel.Text = oldref
                                refexistsbrah = 0
                            End If
                        End If
                    End Using
                End Using
            End Using

        Catch ex As SQLiteException
            MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try
    End Sub
End Class

