VERSION 5.00
Begin VB.Form S 
   Caption         =   "SMS Sample"
   ClientHeight    =   3708
   ClientLeft      =   60
   ClientTop       =   348
   ClientWidth     =   6228
   LinkTopic       =   "Form1"
   ScaleHeight     =   3708
   ScaleWidth      =   6228
   StartUpPosition =   3  'Windows Default
   Begin VB.TextBox Sender 
      Height          =   285
      Left            =   2160
      TabIndex        =   8
      Text            =   "Quicksms.in"
      Top             =   840
      Width           =   2295
   End
   Begin VB.Timer Timer1 
      Enabled         =   0   'False
      Interval        =   1000
      Left            =   3240
      Top             =   240
   End
   Begin VB.Timer Timer2 
      Interval        =   1000
      Left            =   2760
      Top             =   240
   End
   Begin VB.TextBox mobile 
      Height          =   285
      Left            =   2160
      TabIndex        =   0
      Top             =   1200
      Width           =   2295
   End
   Begin VB.TextBox message 
      Height          =   1215
      Left            =   2160
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   1
      Top             =   1800
      Width           =   2295
   End
   Begin VB.CommandButton cmd_send 
      Caption         =   "Send SMS"
      Height          =   375
      Left            =   720
      TabIndex        =   2
      Top             =   3120
      Width           =   1335
   End
   Begin VB.CommandButton cmd_clear 
      Caption         =   "Clear"
      Height          =   375
      Left            =   2160
      TabIndex        =   3
      Top             =   3120
      Width           =   1215
   End
   Begin VB.Label Label2 
      AutoSize        =   -1  'True
      Caption         =   "Sender"
      BeginProperty Font 
         Name            =   "Verdana"
         Size            =   9.6
         Charset         =   0
         Weight          =   700
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   240
      Left            =   840
      TabIndex        =   7
      Top             =   840
      Width           =   765
   End
   Begin VB.Label Label1 
      AutoSize        =   -1  'True
      Caption         =   "Please set your username and password in sms.ini file"
      BeginProperty Font 
         Name            =   "Verdana"
         Size            =   9.6
         Charset         =   0
         Weight          =   700
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   240
      Left            =   240
      TabIndex        =   6
      Top             =   360
      Width           =   5895
   End
   Begin VB.Label Label3 
      AutoSize        =   -1  'True
      Caption         =   "Dest Number"
      BeginProperty Font 
         Name            =   "Verdana"
         Size            =   9.6
         Charset         =   0
         Weight          =   700
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   240
      Left            =   240
      TabIndex        =   5
      Top             =   1200
      Width           =   1410
   End
   Begin VB.Label Label4 
      AutoSize        =   -1  'True
      Caption         =   "Message"
      BeginProperty Font 
         Name            =   "Verdana"
         Size            =   9.6
         Charset         =   0
         Weight          =   700
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   240
      Left            =   840
      TabIndex        =   4
      Top             =   1800
      Width           =   960
   End
End
Attribute VB_Name = "S"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False

Dim Response As String



Public Function SendSMS(ByVal SenderID As String, ByVal mobile As String, ByVal message As String) As Boolean
    Dim URL As String
    Dim Resultt As String
    Dim Username As String
    Dim Password As String
    message = Left(message, 160)
    Username = ReadINI("SMSAccount", "Username", "username")
    Password = ReadINI("SMSAccount", "Password", "password")
    mobile = Right(mobile, 10)
    mobile = "91" & mobile
    
    URL = "http://quicksms.in/sendsms.asp?user=" & Username & "&password=" & Password & "&PhoneNumber=" & mobile & "&sender=" & SenderID & "&Text=" & URLEncode(message)
    Resultt = GetUrlSource(URL)
    If InStr(1, Resultt, "Submitted") > 0 Then
        SendSMS = True
        Response = Resultt
    Else
        SendSMS = False
        Response = Resultt
    End If
    Me.MousePointer = 0
End Function


Private Sub cmd_clear_Click()
    mobile = ""
    message = ""
End Sub

Private Sub cmd_send_Click()
    Dim Status As Boolean
    Status = SendSMS(Sender.Text, mobile.Text, message.Text)
    If Status = True Then
        MsgBox "Message sending successfull!" & vbCrLf & "Server Response: " & Response, vbInformation
    Else
        MsgBox "Message sending failed!" & vbCrLf & "Server Response: " & Response, vbCritical
    End If
End Sub

Private Sub Form_Load()

End Sub
