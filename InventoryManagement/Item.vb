Imports System.ComponentModel

Public Class Item
    Implements INotifyPropertyChanged

    Public Property ID As Integer
    Public Property Name As String
    Public Property Description As String
    Public Property Barcode As String
    Public Property Tag As String
    Public Property Stock As Integer

    Public Property BuyingPrice As Decimal
    Public Property SellingPrice As Decimal
    Public Property Location As String
    Public Property TagID As Integer




    Private _quantity As Integer
    Public Property Quantity As Integer


        Get
            Return _quantity
        End Get
        Set(value As Integer)
            If _quantity <> value Then
                _quantity = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Quantity)))
            End If
        End Set
    End Property
    Public Property Type As String
    Public Property Discount As Decimal


    Public Property Total As Decimal
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

End Class
