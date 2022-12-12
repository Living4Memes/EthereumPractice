using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

namespace EthereumLab1
{
      public partial class Form1 : Form
      {
            private const string CONTRACT_ADDRESS = "0xD7a25C90079DDd2f37b5a96ABDa23b5EC51B71D4";
            private static HexBigInteger GASLIMIT = new HexBigInteger(300000);
            private readonly Web3 _web3Provider = new Web3();
            private readonly Contract _contract;
            private readonly List<string> _users = new List<string>()
            {
                  "0xc92E162b2Ea873f48C37483264633fbc2f14e623",
                  "0xa90A96A478A56A6936676030097457F5aA7C755f",
                  "0x5fA52d4664f4847c00725f2aE7b8540C59164B42",
                  "0x858920b98905084B701c920ef2a76b1e80DCBe57",
                  "0x52fAF86F79CFEF232b4209D376695C3B9af5b980",
                  "0x479C69F633D6C7Faa548Fc4326852a642A7D793a",
                  "0x7B70E62B49f4156ad50B607007f2fDbc8B33BD0c",
                  "0xe6684885d7E2aea6D521Ab3Fb9381f0f8E857A7b",
                  "0xCd85BD254f6Ab9F99DCe7B8593229e91F14f4D36",
                  "0x8A0f4eAa87B4A504fD182E1Ce21125d7f455892D",
            };
            private readonly Dictionary<string, Function> _functions;

            internal string CurrentUser => _users[comboBox1.SelectedIndex];

            public Form1()
            {
                  InitializeComponent();

                  _contract = _web3Provider.Eth.GetContract(Properties.Resources.ContractAbi, CONTRACT_ADDRESS);
                  _functions = new Dictionary<string, Function>()
                  {
                        { "getRealEstate",  _contract.GetFunction("getRealEstate")},
                        { "addRealEstate",  _contract.GetFunction("addRealEstate")},
                        { "changeOwner",  _contract.GetFunction("changeOwner")},
                        { "changeArea",  _contract.GetFunction("changeArea")},
                        { "setRestrictions",  _contract.GetFunction("setRestrictions")},
                        { "sendPresent",  _contract.GetFunction("sendPresent")},
                        { "acceptPresent",  _contract.GetFunction("acceptPresent")},
                        { "sendSellOffer",  _contract.GetFunction("sendSellOffer")},
                        { "acceptSellOffer",  _contract.GetFunction("acceptSellOffer")},
                  };

                  foreach (string user in _users)
                  {
                        comboBox1.Items.Add(user);
                        addOwner.Items.Add(user);
                        changeOwner.Items.Add(user);
                        presentRecipient.Items.Add(user);
                  }
                  comboBox1.SelectedIndex = 0;
                  addOwner.SelectedIndex = 0;
                  presentRecipient.SelectedIndex = 0;
                  textBox1.Text = "111";
                  
                  addRestrictions.SelectedIndex = 0;
            }

            private RealEstateDTO GetObjectInfo(int id)
            {
                  return _functions["getRealEstate"].CallDeserializingToObjectAsync<RealEstateDTO>(id).Result;
            }

            private string AddObject(int id, string owner, string reAddress, int area, bool restricted)
            {
                  return _functions["addRealEstate"].SendTransactionAsync(CurrentUser, GASLIMIT, new HexBigInteger(0), id, owner, reAddress, area, restricted).Result;
            }
            private string ChangeOwner(int id, string newOwner)
            {
                  return _functions["changeOwner"].SendTransactionAsync(CurrentUser, GASLIMIT, new HexBigInteger(0), id, newOwner).Result;
            }
            private string ChangeArea(int id, int newArea)
            {
                  return _functions["changeArea"].SendTransactionAsync(CurrentUser, GASLIMIT, new HexBigInteger(0), id, newArea).Result;
            }
            private string SetRestrictions(int id, bool restricted)
            {
                  return _functions["setRestrictions"].SendTransactionAsync(CurrentUser, GASLIMIT, new HexBigInteger(0), id, restricted).Result;
            }
            private string SendPresent(int id, string recipient)
            {
                  return _functions["sendPresent"].SendTransactionAsync(CurrentUser, GASLIMIT, new HexBigInteger(0), id, recipient).Result;
            }
            private string AcceptPresent(int id)
            {
                  return _functions["acceptPresent"].SendTransactionAsync(CurrentUser, GASLIMIT, new HexBigInteger(0), id).Result;
            }
            private string SendSellOffer(int id, int wei)
            {
                  return _functions["sendSellOffer"].SendTransactionAsync(CurrentUser, GASLIMIT, new HexBigInteger(0), id, wei).Result;
            }
            private string AcceptSellOffer(int id, int wei)
            {
                  return _functions["acceptSellOffer"].SendTransactionAsync(CurrentUser, GASLIMIT, new HexBigInteger(wei), id).Result;
            }
            private string ChangeObjectInfo(RealEstateDTO info, RealEstateDTO newInfo)
            {
                  int id = 0;
                  if (!int.TryParse(changeId.Text, out id))
                        return "Wrong ID";

                  try
                  {
                        if (info.Owner != newInfo.Owner)
                              ChangeOwner(id, newInfo.Owner);
                        if (info.Area != newInfo.Area)
                              ChangeArea(id, (int)newInfo.Area);
                        if (info.Restricted != newInfo.Restricted)
                              SetRestrictions(id, newInfo.Restricted);
                  }
                  catch (Exception ex) { ShowErrorMessage(ex); return "Error"; }

                  return "Success!";
            }

            [FunctionOutput]
            public class RealEstateDTO : IFunctionOutputDTO
            {
                  [Parameter("address", "owner", 1)]
                  public virtual string Owner { get; set; }
                  [Parameter("string", "reAddress", 2)]
                  public virtual string ReAddress { get; set; }
                  [Parameter("uint256", "area", 3)]
                  public virtual BigInteger Area { get; set; }
                  [Parameter("bool", "restricted", 4)]
                  public virtual bool Restricted { get; set; }
            }

            private void ShowErrorMessage(Exception ex)
            {
                  MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            private void textBox1_TextChanged(object sender, EventArgs e)
            {
                  int id = 0;
                  if (!int.TryParse(textBox1.Text, out id))
                        return;

                  RealEstateDTO data = GetObjectInfo(id);
                  ownerInfo.Text = data.Owner;
                  addressInfo.Text = data.ReAddress;
                  areaInfo.Text = data.Area.ToString();
                  restrictionsInfo.Text = data.Restricted ? "Есть" : "Нет";
            }

            private void button1_Click(object sender, EventArgs e)
            {
                  int id, area;
                  if (!int.TryParse(addId.Text, out id) || !int.TryParse(addArea.Text, out area))
                  {
                        ShowErrorMessage(new Exception("Неправильные входные данные"));
                        return;
                  }
                  try { AddObject(id, _users[addOwner.SelectedIndex], addAddress.Text, area, Convert.ToBoolean(addRestrictions.SelectedIndex)); }
                  catch (Exception ex) { ShowErrorMessage(ex); }
            }

            private void changeId_TextChanged(object sender, EventArgs e)
            {
                  int id = 0;
                  if (!int.TryParse(changeId.Text, out id))
                        return;

                  RealEstateDTO data = GetObjectInfo(id);
                  changeOwner.Text = data.Owner;
                  changeAddress.Text = data.ReAddress;
                  changeArea.Text = data.Area.ToString();
                  changeRestrictions.Text = data.Restricted ? "Есть" : "Нет";
            }

            private void button2_Click(object sender, EventArgs e)
            {
                  int id, area;
                  if (!int.TryParse(changeId.Text, out id) || !int.TryParse(changeArea.Text, out area))
                        return;

                  try {
                        ChangeObjectInfo(GetObjectInfo(id), new RealEstateDTO()
                        { Owner = _users[changeOwner.SelectedIndex], Area = area, ReAddress = changeAddress.Text, Restricted = Convert.ToBoolean(changeRestrictions.SelectedIndex) });
                  }
                  catch (Exception ex) { ShowErrorMessage(ex); }
            }

            private void button3_Click(object sender, EventArgs e)
            {
                  int id;
                  if (!int.TryParse(sendPresentId.Text, out id))
                        return;

                  try { SendPresent(id, _users[presentRecipient.SelectedIndex]); }
                  catch (Exception ex) { ShowErrorMessage(ex); }
            }

            private void button4_Click(object sender, EventArgs e)
            {
                  int id;
                  if (!int.TryParse(collectPresentId.Text, out id))
                        return;

                  try { AcceptPresent(id); }
                  catch (Exception ex) { ShowErrorMessage(ex); }
            }

            private void button5_Click(object sender, EventArgs e)
            {
                  int id, wei;
                  if (!int.TryParse(sellId.Text, out id) || !int.TryParse(sellPrice.Text, out wei))
                        return;
                  
                  if (wei <= 0)
                        ShowErrorMessage(new ArgumentException("Стоимость должна быть больше нуля"));
                  else
                        try { SendSellOffer(id, wei); }
                        catch (Exception ex) { ShowErrorMessage(ex); }
            }

            private void button6_Click(object sender, EventArgs e)
            {
                  int id, wei;
                  if (!int.TryParse(buyId.Text, out id) || !int.TryParse(buyValue.Text, out wei))
                        return;
                  
                  if (wei <= 0)
                        ShowErrorMessage(new ArgumentException("Сумма должна быть больше нуля"));
                  else
                        try { AcceptSellOffer(id, wei); }
                        catch (Exception ex) { ShowErrorMessage(ex); }
            }

            private void button7_Click(object sender, EventArgs e)
            {
                  int id = 0;
                  if (!int.TryParse(textBox1.Text, out id))
                        return;

                  RealEstateDTO data = GetObjectInfo(id);
                  ownerInfo.Text = data.Owner;
                  addressInfo.Text = data.ReAddress;
                  areaInfo.Text = data.Area.ToString();
                  restrictionsInfo.Text = data.Restricted ? "Есть" : "Нет";
            }
      }
}
