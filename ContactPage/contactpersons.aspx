<%@ Page Title="Contact Person" Language="C#" MasterPageFile="~/ac.master" AutoEventWireup="true" CodeFile="contactpersons.aspx.cs" Inherits="contactpersons" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">


    <span id="s" runat="server"></span>

    <div>
        <span id="spnMsg" runat="server" style="color: red; font-weight: bold" visible="false"></span>
    </div>

    <asp:GridView ID="gvContactPerson" runat="server" AutoGenerateColumns="false" PageSize="50" DataKeyNames="deviceid , cpid"
        AllowPaging="true" CssClass="table table-striped table-bordered  textcenter" OnRowDeleting="gvContactPerson_RowDeleting" OnRowDataBound="gvContactPerson_RowDataBound">
        <Columns>
            <%-- <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox ID="chkGroupCotrol" runat="server" />
                    </ItemTemplate>
                    <HeaderStyle HorizontalAlign="Center" />
                </asp:TemplateField>--%>
            <asp:TemplateField HeaderText="SR No.">
                <ItemTemplate>
                    <asp:Label ID="lblSN" runat="server" Text='<%#Container.DataItemIndex+1 %>'></asp:Label>
                </ItemTemplate>
                <HeaderStyle HorizontalAlign="Center" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Name">
                <ItemTemplate>
                    <asp:Label ID="lblName" runat="server" Text='<%# Eval("name") %>'></asp:Label>
                </ItemTemplate>
                <HeaderStyle HorizontalAlign="Center" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Mobile No">
                <ItemTemplate>
                    <asp:Label ID="lblMno" runat="server" Text='<%# Eval("mobile") %>'></asp:Label>
                </ItemTemplate>
                <HeaderStyle HorizontalAlign="Center" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Address">
                <ItemTemplate>
                    <asp:Label ID="lblAddress" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                </ItemTemplate>
                <HeaderStyle HorizontalAlign="Center" />
            </asp:TemplateField>
            <asp:TemplateField HeaderStyle-HorizontalAlign="Left" HeaderText="Option">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkDelete" runat="server" Text="Delete" CommandName="Delete" CausesValidation="false" OnClientClick="return confirm('Are you sure you want to delete this contact person?');"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerStyle HorizontalAlign="Right" CssClass="pagination-ys" />
    </asp:GridView>

    <div>
        <span id="spnSMSSend" runat="server" style="color: green; font-weight: bold" visible="false"></span>
    </div>
    <div class="row">
        <div class=" col-md-12 col-lg-12 ">
            <table class="table table-user-information">
                <tbody>
                    <tr>
                        <td>Device ID :</td>
                        <td>
                            <asp:TextBox ID="txtDeviceId" CssClass="form-control" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>Zone - Ward :</td>
                        <td>
                            <asp:TextBox ID="txtZone" CssClass="form-control" runat="server"></asp:TextBox>
                        </td>
                    </tr>

                    <tr>
                        <td>Area & Location :</td>
                        <td>
                            <asp:TextBox ID="txtAreaLocation" CssClass="form-control" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>Status : </td>
                        <td>
                            <asp:TextBox ID="txtStatus" CssClass="form-control" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>Mobile No :</td>
                        <td>
                            <asp:TextBox ID="txtSendMobile" CssClass="form-control" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RFVtxtSendMobile" runat="server" ControlToValidate="txtSendMobile" Display="Dynamic" ErrorMessage="Please enter the mobile no" ValidationGroup="m1"></asp:RequiredFieldValidator>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>
                            <asp:Button ID="btnSendSMS" runat="server" Text="Send SMS Alert" CssClass="btn-danger" ValidationGroup="m1" OnClick="btnSendSMS_Click" />
                        </td>
                    </tr>

                </tbody>
            </table>
        </div>
    </div>

    <div class="panel panel-info">
        <div class="panel-heading">
            <h3 class="panel-title" id="h3Name" runat="server">Add New Contact Person</h3>
        </div>
        <div class="panel-body">
            <div class="row">
                <div class=" col-md-12 col-lg-12 ">
                    <table class="table table-user-information">
                        <tbody>
                            <tr>
                                <td colspan="3">
                                    <b><span id="spnMessage" runat="server"></span></b>
                                </td>
                            </tr>
                            <tr>
                                <td>Name :</td>
                                <td>
                                    <asp:TextBox ID="txtName" CssClass="form-control" runat="server"></asp:TextBox></td>
                                <td>
                                    <asp:RequiredFieldValidator ID="RFVtxtName" runat="server" ControlToValidate="txtName" ErrorMessage="Please Enter Name" ValidationGroup="cp"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td>Mobile :</td>
                                <td>
                                    <asp:TextBox ID="txtMobile" CssClass="form-control" runat="server"></asp:TextBox>
                                    <cc:FilteredTextBoxExtender ID="FTBtxtMobile" runat="server" TargetControlID="txtMobile" FilterMode="ValidChars" ValidChars="0123456789+"></cc:FilteredTextBoxExtender>
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="RFVtxtMobile" runat="server" ControlToValidate="txtMobile" ErrorMessage="Please Enter Mobile" ValidationGroup="cp"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td>Address :</td>
                                <td>
                                    <asp:TextBox ID="txtAddress" CssClass="form-control" runat="server"></asp:TextBox></td>
                                <td>
                                    <asp:RequiredFieldValidator ID="RFVtxtAddress" runat="server" ControlToValidate="txtAddress" ErrorMessage="Please Enter Address" ValidationGroup="cp"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                                <td>
                                    <asp:Button ID="btnAddNewAddress" CssClass="btn btn-primary" runat="server" Text="Submit" ValidationGroup="cp" OnClick="btnAddNewAddress_Click" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

