using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;
using System.Threading.Tasks;
using Memory;
using ENet.Managed;
using System.Text;
using System.Net;
using System.Linq;
using Guna.UI2.WinForms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.Mail;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace GrowDBG
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region accountstrings
        public static string tankIDName = "";
        public static string tankIDPass = "";
        public static string game_version = "3.38";
        public static string country = "ch";
        public static string requestedName = "PunchBill";
        public static int token = 0;
        public static int Growtopia_Port = 17273; // todo auto get port
        public static string Growtopia_IP = "213.179.209.168";
        public static string Growtopia_Master_IP = "213.179.209.168";
        public static int Growtopia_Master_Port = 17273;
        public static int userID = 0;
        public static int lmode = 0;
        public static string macc = "02:15:11:20:30:05";
        public static string doorid = "";
        #endregion
        public static PacketSending packetSender = new PacketSending();
        public class ItemDatabase
        {
            public struct ItemDefinition
            {
                public short id;
                public byte editType;
                public byte editCategory;
                public byte actionType;
                public byte hitSound;
                public string itemName;
                public string fileName;
                public int texHash;
                public byte itemKind;
                public byte texX;
                public byte texY;
                public byte sprType;
                public byte isStripey;
                public byte collType;
                public byte hitsTaken;
                public byte dropChance;
                public int clothingType;
                public short rarity;
                public short toolKind;
                public string audioFile;
                public int audioHash;
                public short audioVol;
                public byte seedBase;
                public byte seedOver;
                public byte treeBase;
                public byte treeOver;
                // Colors are stored in ARGB.
                public byte color1R, color1G, color1B, color1A;
                public byte color2R, color2G, color2B, color2A;
                public short ing1, ing2;
                public int growTime;
                public string extraUnk01;
                public string extraUnk02;
                public string extraUnk03;
                public string extraUnk04;
                public string extraUnk05;
                public string extraUnk11;
                public string extraUnk12;
                public string extraUnk13;
                public string extraUnk14;
                public string extraUnk15;
                public short extraUnkShort1;
                public short extraUnkShort2;
                public int extraUnkInt1;
            };

            public static List<ItemDefinition> itemDefs = new List<ItemDefinition>();

            public static bool isBackground(int itemID) // thanks for the dev iProgramInCpp for telling me a reliable method on how to determine between foreground and background in GT.
            {
                ItemDefinition def = GetItemDef(itemID);
                byte actType = def.actionType;
                return (actType == 18 || actType == 23 || actType == 28);
            }
            public static ItemDefinition GetItemDef(int itemID)
            {
                if (itemID < 0 || itemID > (int)itemDefs.Count()) return itemDefs[0];
                ItemDefinition def = itemDefs[itemID];
                if (def.id != itemID)
                {
                    // For some reason, something is off.
                    foreach (var d in itemDefs)
                    {
                        if (d.id == itemID) return d;
                    }
                }
                return def;
            }

            public static bool RequiresTileExtra(int id)
            {
                ItemDefinition def = GetItemDef(id);
                return
                    def.actionType == 2 || // Door
                    def.actionType == 3 || // Lock
                    def.actionType == 10 || // Sign
                    def.actionType == 13 || // Main Door
                    def.actionType == 19 || // Seed
                    def.actionType == 26 || // Portal
                    def.actionType == 33 || // Mailbox
                    def.actionType == 34 || // Bulletin Board
                    def.actionType == 36 || // Dice Block
                    def.actionType == 36 || // Roshambo Block
                    def.actionType == 38 || // Chemical Source
                    def.actionType == 40 || // Achievement Block
                    def.actionType == 43 || // Sungate
                    def.actionType == 46 ||
                    def.actionType == 47 ||
                    def.actionType == 49 ||
                    def.actionType == 50 ||
                    def.actionType == 51 || // Bunny Egg
                    def.actionType == 52 ||
                    def.actionType == 53 ||
                    def.actionType == 54 || // Xenonite
                    def.actionType == 55 || // Phone Booth
                    def.actionType == 56 || // Crystal
                    def.id == 2246 || // Crystal
                    def.actionType == 57 || // Crime In Progress
                    def.actionType == 59 || // Spotlight
                    def.actionType == 61 ||
                    def.actionType == 62 ||
                    def.actionType == 63 || // Fish Wall Port
                    def.id == 3760 || // Data Bedrock
                    def.actionType == 66 || // Forge
                    def.actionType == 67 || // Giving Tree
                    def.actionType == 73 || // Sewing Machine
                    def.actionType == 74 ||
                    def.actionType == 76 || // Painting Easel
                    def.actionType == 78 || // Pet Trainer (WHY?!)
                    def.actionType == 80 || // Lock-Bot (Why?!)
                    def.actionType == 81 ||
                    def.actionType == 83 || // Display Shelf
                    def.actionType == 84 ||
                    def.actionType == 85 || // Challenge Timer
                    def.actionType == 86 || // Challenge Start/End Flags
                    def.actionType == 87 || // Fish Wall Mount
                    def.actionType == 88 || // Portrait
                    def.actionType == 89 ||
                    def.actionType == 91 || // Fossil Prep Station
                    def.actionType == 93 || // Howler
                    def.actionType == 97 || // Storage Box Xtreme / Untrade-a-box
                    def.actionType == 100 || // Geiger Charger
                    def.actionType == 101 ||
                    def.actionType == 111 || // Magplant
                    def.actionType == 113 || // CyBot
                    def.actionType == 115 || // Lucky Token
                    def.actionType == 116 || // GrowScan 9000 ???
                    def.actionType == 127 || // Temp. Platform
                    def.actionType == 130 ||
                    (def.id % 2 == 0 && def.id >= 5818 && def.id <= 5932) ||
                    // ...
                    false;
            }

            public void SetupItemDefs()
            {
                string a = File.ReadAllText("include/base.txt");
                List<string> aaa = a.Split('|').ToList();
                if (aaa.Count < 3) return;
                int itemCount = -1;
                int.TryParse(aaa[2], out itemCount);
                if (itemCount == -1) return;
                short id = 0;
                itemDefs.Clear();
                ItemDefinition def = new ItemDefinition();
                using (StreamReader sr = File.OpenText("include/item_defs.txt"))
                {
                    string s = String.Empty;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.Length < 2) continue;
                        if (s.Contains("//")) continue;
                        List<string> infos = s.Split('\\').ToList();
                        if (infos[0] != "add_item") continue;

                        def.id = short.Parse(infos[1]);
                        def.actionType = byte.Parse(infos[4]);
                        def.itemName = infos[6];

                        if (def.id != id)
                        {
                            // unordered db item, can cause problems!!

                        }
                        itemDefs.Add(def);
                        id++;
                    }
                }
            }
        }
            public class Player //NetAvatar
        {


            public string name = "";
            public string country = "";
            public int netID = 0;
            public int userID = 0;
            public int invis = 0;
            public int mstate = 0;
            public int smstate = 0;
            public int X, Y = 0;
            public bool didClothingLoad = false; // unused now
            public bool didCharacterStateLoad = false; // unused now
            public Inventory inventory; // should only not be null if player is local.
            public void SerializePlayerInventory(byte[] inventoryData)
            {
                int invPacketSize = inventoryData.Length;
                inventory.version = inventoryData[0];
                inventory.backpackSpace = BitConverter.ToInt16(inventoryData, 1);
                int inventoryitemCount = BitConverter.ToInt16(inventoryData, 5); // trade exceeding
                inventory.items = new InventoryItem[inventoryitemCount];

                for (int i = 0; i < inventoryitemCount; i++)
                {
                    int pos = 7 + i * 4;
                    inventory.items[i].itemID = BitConverter.ToUInt16(inventoryData, pos);
                    inventory.items[i].amount = BitConverter.ToInt16(inventoryData, pos + 2);
                }
            }
        };


        public struct Inventory
        {
            public byte version;
            public short backpackSpace;
            public InventoryItem[] items;
        }
        public struct InventoryItem
        {
            public short amount;
            public ushort itemID;
            public byte flags; // 8-bits reserved.
        }

        public struct Tile
        {
            public int x, y;
            public ushort fg, bg;
            public int tileState;
            public byte[] tileExtra; // might be unused.
            public string str_1; // might be unused.
            public byte type;
        };


        class VariantList
        {
            // this class has been entirely made by me, based on the code available on the gt bot of anybody :)
            [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
            public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

            public struct VarList
            {
                public string FunctionName;
                public int netID;
                public uint delay;
                public object[] functionArgs;
            };

            public enum OnSendToServerArgs
            {
                port = 1,
                token,
                userId,
                IPWithExtraData = 4
            };

            public static byte[] get_extended_data(byte[] pktData)
            {
                return pktData.Skip(56).ToArray();
            }

            public static byte[] get_struct_data(byte[] package)
            {
                int packetLen = package.Length;
                if (packetLen >= 0x3c)
                {
                    byte[] structPackage = new byte[packetLen - 4];
                    Array.Copy(package, 4, structPackage, 0, packetLen - 4);
                    int p2Len = BitConverter.ToInt32(package, 56);
                    if (((byte)(package[16]) & 8) != 0)
                    {
                        if (packetLen < p2Len + 60)
                        {
                            ////MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): (ERROR) Too small extended packet to be valid!\n");
                        }
                    }
                    else
                    {
                        Array.Copy(BitConverter.GetBytes(0), 0, package, 56, 4);
                    }
                    return structPackage;
                }
                return null;
            }

            public static VarList GetCall(byte[] package)
            {

                VarList varList = new VarList();
                //if (package.Length < 60) return varList;
                int pos = 0;
                //varList.netID = BitConverter.ToInt32(package, 8);
                //varList.delay = BitConverter.ToUInt32(package, 24);
                byte argsTotal = package[pos];
                pos++;
                if (argsTotal > 7) return varList;
                varList.functionArgs = new object[argsTotal];

                for (int i = 0; i < argsTotal; i++)
                {
                    varList.functionArgs[i] = 0; // just to be sure...
                    byte index = package[pos]; pos++; // pls dont bully sm
                    byte type = package[pos]; pos++;


                    switch (type)
                    {
                        case 1:
                            {
                                float vFloat = BitConverter.ToUInt32(package, pos); pos += 4;
                                varList.functionArgs[index] = vFloat;
                                break;
                            }
                        case 2: // string
                            int strLen = BitConverter.ToInt32(package, pos); pos += 4;
                            string v = string.Empty;
                            v = Encoding.ASCII.GetString(package, pos, strLen); pos += strLen;

                            if (index == 0)
                                varList.FunctionName = v;

                            if (index > 0)
                            {
                                if (varList.FunctionName == "OnSendToServer") // exceptionary function, having it easier like this :)
                                {
                                    doorid = v.Substring(v.IndexOf("|") + 1); // doorid
                                    if (v.Length >= 8)
                                        v = v.Substring(0, v.IndexOf("|"));
                                }

                                varList.functionArgs[index] = v;
                            }
                            break;
                        case 5: // uint
                            uint vUInt = BitConverter.ToUInt32(package, pos); pos += 4;
                            varList.functionArgs[index] = vUInt;
                            break;
                        case 9: // int (can hold negative values, of course they are always casted but its just a sign from the server that the value was intended to hold negative values as well)
                            int vInt = BitConverter.ToInt32(package, pos); pos += 4;
                            varList.functionArgs[index] = vInt;
                            break;
                        default:
                            break;
                    }
                }
                return varList;
            }
        }
        public class PacketSending
        {
            private Random rand = new Random();
            public void SendData(byte[] data, ENetPeer peer, ENetPacketFlags flag = ENetPacketFlags.Reliable)
            {

                if (peer == null) return;
                if (peer.State != ENetPeerState.Connected) return;

                if (rand.Next(0, 1) == 0) peer.Send(data, 0, flag);
                else peer.Send(data, 1, flag);
            }

            public void SendPacketRaw(int type, byte[] data, ENetPeer peer, ENetPacketFlags flag = ENetPacketFlags.Reliable)
            {
                byte[] packetData = new byte[data.Length + 5];
                Array.Copy(BitConverter.GetBytes(type), packetData, 4);
                Array.Copy(data, 0, packetData, 4, data.Length);
                SendData(packetData, peer);
            }

            public void SendPacket(int type, string str, ENetPeer peer, ENetPacketFlags flag = ENetPacketFlags.Reliable)
            {
                SendPacketRaw(type, Encoding.ASCII.GetBytes(str.ToCharArray()), peer);
            }

            public void SecondaryLogonAccepted(ENetPeer peer)
            {
                SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, string.Empty, peer);
            }

            public void InitialLogonAccepted(ENetPeer peer)
            {
                SendPacket((int)NetTypes.NetMessages.SERVER_HELLO, string.Empty, peer);
            }
        }
        public  class NetTypes
        {
            public enum PacketTypes
            {
                PLAYER_LOGIC_UPDATE = 0,
                CALL_FUNCTION,
                UPDATE_STATUS,
                TILE_CHANGE_REQ,
                LOAD_MAP,
                TILE_EXTRA,
                TILE_EXTRA_MULTI,
                TILE_ACTIVATE,
                APPLY_DMG,
                INVENTORY_STATE,
                ITEM_ACTIVATE,
                ITEM_ACTIVATE_OBJ,
                UPDATE_TREE,
                MODIFY_INVENTORY_ITEM,
                MODIFY_ITEM_OBJ,
                APPLY_LOCK,
                UPDATE_ITEMS_DATA,
                PARTICLE_EFF,
                ICON_STATE,
                ITEM_EFF,
                SET_CHARACTER_STATE,
                PING_REPLY,
                PING_REQ,
                PLAYER_HIT,
                APP_CHECK_RESPONSE,
                APP_INTEGRITY_FAIL,
                DISCONNECT,
                BATTLE_JOIN,
                BATTLE_EVENT,
                USE_DOOR,
                PARENTAL_MSG,
                GONE_FISHIN,
                STEAM,
                PET_BATTLE,
                NPC,
                SPECIAL,
                PARTICLE_EFFECT_V2,
                ARROW_TO_ITEM,
                TILE_INDEX_SELECTION,
                UPDATE_PLAYER_TRIBUTE
            };

            public enum NetMessages
            {
                UNKNOWN = 0,
                SERVER_HELLO,
                GENERIC_TEXT,
                GAME_MESSAGE,
                GAME_PACKET,
                ERROR,
                TRACK,
                LOG_REQ,
                LOG_RES
            };

        }

        private static char[] hexmap = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        private static string hexStr(byte data)
        {
            string s = new string(' ', 2);
            s = StringFunctions.ChangeCharacter(s, 0, hexmap[(data & 0xF0) >> 4]);
            s = StringFunctions.ChangeCharacter(s, 1, hexmap[data & 0x0F]);
            return s;
        }

        private static string generateMeta()
        {
            StringBuilder x = new StringBuilder();
            for (int i = 0; i < 9; i++)
            {
                x.Append(hexStr((byte)RandomNumbers.NextNumber()));
            }
            x.Append(".com");
            return x.ToString();
        }

        private static string generateMac()
        {
            StringBuilder x = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                x.Append(hexStr((byte)RandomNumbers.NextNumber()));
                if (i != 5)
                {
                    x.Append(":");
                }
            }
            return x.ToString();
        }

        private static string generateRid()
        {
            StringBuilder x = new StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                x.Append(hexStr((byte)RandomNumbers.NextNumber()).ToUpper());
            }
            return x.ToString();
        }
        internal static class RandomNumbers
        {
            private static System.Random r;

            public static int NextNumber()
            {
                if (r == null)
                    Seed();

                return r.Next();
            }

            public static int NextNumber(int ceiling)
            {
                if (r == null)
                    Seed();

                return r.Next(ceiling);
            }

            public static void Seed()
            {
                r = new System.Random();
            }

            public static void Seed(int seed)
            {
                r = new System.Random(seed);
            }
        }
        internal static class StringFunctions
        {
            //------------------------------------------------------------------------------------
            //	This method allows replacing a single character in a string, to help convert
            //	C++ code where a single character in a character array is replaced.
            //------------------------------------------------------------------------------------
            public static string ChangeCharacter(string sourceString, int charIndex, char newChar)
            {
                return (charIndex > 0 ? sourceString.Substring(0, charIndex) : "")
                    + newChar.ToString() + (charIndex < sourceString.Length - 1 ? sourceString.Substring(charIndex + 1) : "");
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'isxdigit' (and 'iswxdigit').
            //------------------------------------------------------------------------------------
            public static bool IsXDigit(char character)
            {
                if (char.IsDigit(character))
                    return true;
                else if ("ABCDEFabcdef".IndexOf(character) > -1)
                    return true;
                else
                    return false;
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'strchr' (and 'wcschr').
            //------------------------------------------------------------------------------------
            public static string StrChr(string stringToSearch, char charToFind)
            {
                int index = stringToSearch.IndexOf(charToFind);
                if (index > -1)
                    return stringToSearch.Substring(index);
                else
                    return null;
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'strrchr' (and 'wcsrchr').
            //------------------------------------------------------------------------------------
            public static string StrRChr(string stringToSearch, char charToFind)
            {
                int index = stringToSearch.LastIndexOf(charToFind);
                if (index > -1)
                    return stringToSearch.Substring(index);
                else
                    return null;
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'strstr' (and 'wcsstr').
            //------------------------------------------------------------------------------------
            public static string StrStr(string stringToSearch, string stringToFind)
            {
                int index = stringToSearch.IndexOf(stringToFind);
                if (index > -1)
                    return stringToSearch.Substring(index);
                else
                    return null;
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'strtok' (and 'wcstok').
            //	Note that the .NET string 'Split' method cannot be used to replicate 'strtok' since
            //	it doesn't allow changing the delimiters between each token retrieval.
            //------------------------------------------------------------------------------------
            private static string activeString;
            private static int activePosition;
            public static string StrTok(string stringToTokenize, string delimiters)
            {
                if (stringToTokenize != null)
                {
                    activeString = stringToTokenize;
                    activePosition = -1;
                }

                //the stringToTokenize was never set:
                if (activeString == null)
                    return null;

                //all tokens have already been extracted:
                if (activePosition == activeString.Length)
                    return null;

                //bypass delimiters:
                activePosition++;
                while (activePosition < activeString.Length && delimiters.IndexOf(activeString[activePosition]) > -1)
                {
                    activePosition++;
                }

                //only delimiters were left, so return null:
                if (activePosition == activeString.Length)
                    return null;

                //get starting position of string to return:
                int startingPosition = activePosition;

                //read until next delimiter:
                do
                {
                    activePosition++;
                } while (activePosition < activeString.Length && delimiters.IndexOf(activeString[activePosition]) == -1);

                return activeString.Substring(startingPosition, activePosition - startingPosition);
            }
        }
        /*   public static string CreateLogonPacket(string customGrowID = "test", string customPass = "test")
           {
               string p = string.Empty;
               Random rand = new Random();
               bool requireAdditionalData = false; if (token > 0 || token < 0) requireAdditionalData = true;
               p += "tankIDName|" + (customGrowID + "\n");
               p += "tankIDPass|" + (customPass + "\n");
               p += "requestedName|" + ("PunchBill\n");
               p += "f|1\n";
               p += "protocol|94\n"; //94
               p += "game_version|" + (game_version + "\n");
               p += "fz|6069928\n";
               if (requireAdditionalData) p += "lmode|" + lmode + "\n";
               p += "cbits|0\n";
               p += "player_age|18\n";
               p += "GDPR|1\n";
               p += "hash2|" + rand.Next(-777777777, 777777777).ToString() + "\n";
               p += "meta|"+generateMeta()+"\n"; // soon auto fetch meta etc.
               p += "fhash|-716928004\n";
               p += "rid|"+generateRid()+"\n";
               p += "platformID|0\n";
               p += "deviceVersion|0\n";
               p += "country|" + "us" + "\n";
               p += "hash|" + rand.Next(-777777777, 777777777).ToString() + "\n";
               p += "mac|" + generateMac() + "\n";
               p += "wk|" + generateRid() + "\n";
               if (requireAdditionalData) p += "user|" + (userID.ToString() + "\n");
               if (requireAdditionalData) p += "token|" + (token.ToString() + "\n");
               p += "zf|-496303939";
               return p;
           }*/
        public static string CreateLogonPacket(string customGrowID = "", string customPass = "")
        {
            string p = string.Empty;
            Random rand = new Random();
            bool requireAdditionalData = false; if (token > 0 || token < 0) requireAdditionalData = true;

            if (customGrowID == "")
            {
                if (tankIDName != "")
                {
                    p += "tankIDName|" + (tankIDName + "\n");
                    p += "tankIDPass|" + (tankIDPass + "\n");
                }
            }
            else
            {
                p += "tankIDName|" + (customGrowID + "\n");
                p += "tankIDPass|" + (customPass + "\n");
            }

            p += "requestedName|" + ("Bill\n"); //"Growbrew" + rand.Next(0, 255).ToString() + "\n"
            p += "f|1\n";
            p += "protocol|100\n";//94
            p += "game_version|" + (game_version + "\n");
            p += "fz|" + ("6069928" + "\n");
            p += "lmode|" + "0" + "\n";
            p += "cbits|0\n";
            p += "player_age|57\n";
            p += "GDPR|1\n";
            p += "hash2|" + rand.Next(-777777777, 777777777).ToString() + "\n";
            p += "meta|"+generateMeta()+"\n"; // soon auto fetch meta etc.
            p += "fhash|-716928004\n";
            p += "rid|"+generateRid()+"\n";
            p += "platformID|0\n";
            p += "deviceVersion|0\n";
            p += "country|" + ("ch" + "\n");
            p += "hash|" + rand.Next(-777777777, 777777777).ToString() + "\n";
            p += "user|"+userID+"\n";
            p += "token|"+token+"\n";
            p += "mac|" + generateMac() + "\n";
            p += "wk|" + (generateRid() + "\n");
            p += "zf|-595512788";
            if (requireAdditionalData) p += "user|" + (userID.ToString() + "\n");
            if (requireAdditionalData) p += "token|" + (token.ToString() + "\n");
            if (doorid != "") p += "doorID|" + doorid.ToString() + "\n";
            

            return p;
        }
        public static bool ProgramRunningAsAdmin()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                .IsInRole(WindowsBuiltInRole.Administrator);
        }
        void disablecontrols()
        {
            metroButton1.Enabled = false;
            metroButton1.Text += " (Please run administrator)";
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if(ProgramRunningAsAdmin())
            {
                this.Text += " (Administrator)";
            }
            else
            {
                this.Text += " (No Administrator)";
                disablecontrols();
            }
            Console.WriteLine("[+] Initialized",Console.ForegroundColor = ConsoleColor.Yellow);

            metroTabControl1.SelectedIndex = 0;

        }
        Mem mem = new Mem();
        private async void metroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("Growtopia");
                mem.OpenProcess(processes[0].Id);
                await Task.Delay(220);
                mem.WriteMemory("base+"+metroTextBox1.Text,"bytes", metroTextBox2.Text);
                BYTEstat.Text = "Changed | Byte: "+ metroTextBox1.Text+" Bytes:"+ metroTextBox2.Text;
                BYTEstat.ForeColor = Color.Indigo;
            }
            catch
            {
                BYTEstat.Text = "Error! (May be gt is not working now.)";
                BYTEstat.ForeColor = Color.Orange;
            }

        }
        void changestatus(bool run=false)
        {
            if(run == false)
            {
                status.Text = "Growtopia is not running now!";
                status.ForeColor = Color.Orange;
            }
            else if(run == true)
            {
                status.Text = "Growtopia is running now!";
                status.ForeColor = Color.Indigo;
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Process[] gt = Process.GetProcessesByName("Growtopia");
            if(gt.Length == 0)
            {
                changestatus(false);
                metroButton1.Enabled = false;
            }
            else
            {
                changestatus(true);
                metroButton1.Enabled = true;

            }
        }
        private static ENetHost g_Client;
        private static ENetPeer g_Peer;
        public static void ConnectCurrent()
        {
            if (g_Client == null) return;

            if (g_Client.ServiceThreadStarted)
            {

                if (g_Peer == null)
                {
                    g_Peer = g_Client.Connect(new System.Net.IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 2, 0);
                }
                else if (g_Peer.State == ENetPeerState.Connected)
                {
                    g_Peer.Reset();

                    g_Peer = g_Client.Connect(new System.Net.IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 2, 0);
                }
            }
        }
        static string hash = Convert.ToString((uint)RandomNumbers.NextNumber());
        static string hash2 = Convert.ToString((uint)RandomNumbers.NextNumber());
        
        string all;
        string wrold = "";
        string lvls = "";
        int zdz = 0;
        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.richTextBox1.Text = text;
            }
        }

        void start()
        {
            if(packetwant.Checked == true)
            {
                MessageBox.Show(all);
            }
            SetText(all);
            Thread.Sleep(350);
            this.BeginInvoke(new MethodInvoker(delegate ()
            {
                string[] lines = richTextBox1.Text.Split('\n');
                for(int i = 0; i < lines.Length - 1; i++)
                {
                    if (lines[i].IndexOf("Gems") != -1)
                    {
                        GemBalance.Text = "Gem Balance: "+lines[i].Replace("Gems_balance|","");
                    }
                    if (lines[i].IndexOf("Worldlock") != -1)
                    {
                        WorldLockBalance.Text = "World Lock Count: " + lines[i].Replace("Worldlock_balance|", "");
                    }
                    if (lines[i].IndexOf("Level") != -1)
                    {
                        level.Text = "Level: " + lines[i].Replace("Level|", "");
                    }
                }
            }));


        }
        private void Peer_OnReceive_Client(object sender, ENetPacket e)
        {
            try
            {

                byte[] packet = e.GetPayloadFinal();
                Console.WriteLine("RECEIVE TYPE: " + packet[0].ToString());
                switch (packet[0])
                {
                    case 1: // HELLO server packet.
                        {
                            string username = guna2TextBox1.Text;
                            string pass = guna2TextBox2.Text;
                            // todo add mac optionally, will do that incases aap bypass gets fixed.
                            Console.WriteLine("[ACCOUNT-CHECKER] Logging on " + username + "...");
                            packetSender.SendPacket(2, CreateLogonPacket(username, pass), g_Peer);
                            break;
                        }
                    case 2:
                    case 3:
                        {
                            Console.WriteLine("[ACCOUNT-CHECKER] TEXT PACKET CONTENT:\n" + Encoding.ASCII.GetString(packet.Skip(4).ToArray()));
                            //action|play_sfx
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("action|play_sfx") != -1)
                            {
                                packetSender.SendPacket(3, "action|join_request\nname|" + "xfacts", g_Peer);
                            }
                            if (Encoding.ASCII.GetString(packet).Contains("action|logon_fail"))
                            {
                                ConnectCurrent();
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("password is wrong") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    statss.Text = "Status: Wrong Account.";
                                    g_Peer.Disconnect(0);
                                    guna2Button1.Enabled = true;
                                    guna2TextBox1.Enabled = true;
                                    guna2TextBox2.Enabled = true;
                                }));
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("LOGON ATTEMPTS") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    statss.Text = "Status: need change ip.";
                                    g_Peer.Disconnect(0);
                                    guna2Button1.Enabled = true;
                                    guna2TextBox1.Enabled = true;
                                    guna2TextBox2.Enabled = true;
                                }));
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("Protect") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    statss.Text = "Status: AAP Account.";
                                    g_Peer.Disconnect(0);
                                    guna2Button1.Enabled = true;
                                    guna2TextBox1.Enabled = true;
                                    guna2TextBox2.Enabled = true;
                                }));
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("suspend") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    statss.Text = "Status: Suspended Account.";
                                    g_Peer.Disconnect(0);
                                    guna2Button1.Enabled = true;
                                    guna2TextBox1.Enabled = true;
                                    guna2TextBox2.Enabled = true;
                                }));
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("ban") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    statss.Text = "Status: Banned Account.";
                                    g_Peer.Disconnect(0);
                                    guna2Button1.Enabled = true;
                                    guna2TextBox1.Enabled = true;
                                    guna2TextBox2.Enabled = true;
                                }));
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("maintenance") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    statss.Text = "Status: gt maintenance.";
                                    g_Peer.Disconnect(0);
                                    guna2Button1.Enabled = true;
                                    guna2TextBox1.Enabled = true;
                                    guna2TextBox2.Enabled = true;
                                }));
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("Authenticated|0") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    statss.Text = "Status: err.";
                                    g_Peer.Disconnect(0);//300_WORLD_VISIT
                                    guna2Button1.Enabled = true;
                                    guna2TextBox1.Enabled = true;
                                    guna2TextBox2.Enabled = true;
                                }));
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("300_WORLD_VISIT") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    g_Peer.Disconnect(0);
                                    /*  guna2Button1.Enabled = true;
                                      guna2TextBox1.Enabled = true;
                                      guna2TextBox2.Enabled = true;*/
                                }));
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("logon") != -1)
                            {
                                this.BeginInvoke(new MethodInvoker(delegate ()
                                {
                                    statss.Text = "Status: Trying again.";
                                }));
                                byte[] tankPacket = VariantList.get_struct_data(packet);
                                if (tankPacket[0] == 1)
                                {
                                    VariantList.VarList vList = VariantList.GetCall(VariantList.get_extended_data(tankPacket));
                                    vList.netID = BitConverter.ToInt32(tankPacket, 4);
                                    vList.delay = BitConverter.ToUInt32(tankPacket, 20);


                                    if (vList.FunctionName == "OnSendToServer")
                                    {
                                        string ip = (string)vList.functionArgs[4];

                                        if (ip.Contains("|"))
                                            ip = ip.Substring(0, ip.IndexOf("|"));

                                        int port = (int)vList.functionArgs[1];
                                        userID = (int)vList.functionArgs[3];
                                        token = (int)vList.functionArgs[2];
                                        lmode = (int)vList.functionArgs[5];
                                        Growtopia_IP = ip;
                                        Growtopia_Port = port;
                                        ConnectCurrent();
                                        //  ConnectCurrent();
                                    }
                                }
                            }
                        }
                        break;

                    case 4:
                        {
                            string foundedsmessage = Encoding.ASCII.GetString(packet.ToArray());
                           // Console.WriteLine(foundedsmessage);

                            byte[] tankPacket = VariantList.get_struct_data(packet);
                            byte tankPacketType = tankPacket[0];
                            NetTypes.PacketTypes packetType = (NetTypes.PacketTypes)tankPacketType;
                            Player player = new Player();
                            switch (packetType)
                            {
                                case NetTypes.PacketTypes.INVENTORY_STATE:
                                    {
                                        player.SerializePlayerInventory(VariantList.get_extended_data(tankPacket));
                                        foreach (InventoryItem item in player.inventory.items)
                                        {
                                            ItemDatabase.ItemDefinition itemDef = ItemDatabase.GetItemDef(item.itemID);
                                            Console.WriteLine("ITEM NAME: " + itemDef.itemName + " AMOUNT: " + item.amount);
                                        }
                                        break;
                                    }
                            }
                            /*if (MainForm.logallpackettypes)
                            {
                                GamePacketProton gp = new GamePacketProton();
                                gp.AppendString("OnConsoleMessage");
                                gp.AppendString("`6(PROXY) `wPacket TYPE: " + tankPacketType.ToString());
                                packetSender.SendData(gp.GetBytes(), MainForm.proxyPeer);
                            }*/
                            if (tankPacket[0] == 1)
                            {
                                VariantList.VarList vList = VariantList.GetCall(VariantList.get_extended_data(tankPacket));
                                vList.netID = BitConverter.ToInt32(tankPacket, 4);
                                vList.delay = BitConverter.ToUInt32(tankPacket, 20);

                                Console.WriteLine("VLISTTYPES!!!"+vList.FunctionName);

                                if (vList.FunctionName == "OnSpawn")
                                {
                                    for(int i= 0;i < vList.functionArgs.Length;i++)
                                    {
                                        Console.WriteLine("onspawn argument{"+i.ToString()+"} packet: "+ Encoding.ASCII.GetString(packet.Skip(4).ToArray()));
                                    }
                                    
                                    //Console.WriteLine("OnSpawn: "+foundedmessages);
                                }

                                if (vList.FunctionName == "OnRequestWorldSelectMenu")
                                {
                                    packetSender.SendPacket(3, "action|join_request\nname|" + "xfacts", g_Peer);
                                }

                                if (vList.FunctionName == "OnPlayPositioned")
                                {
                                    Console.WriteLine("OnPlayPositioned packet: " + Encoding.ASCII.GetString(packet.Skip(4).ToArray()));
                                }

                                if (vList.FunctionName == "OnTalkBubble")
                                {
                                    Console.WriteLine("OnTalkBubble packet: " + Encoding.ASCII.GetString(packet.Skip(4).ToArray()));
                                }

                                if (vList.FunctionName == "onShowCaptcha")
                                {
                                    string foundedmessages = Encoding.ASCII.GetString(packet.Skip(4).ToArray());
                                    Console.WriteLine("Captcha: " + foundedmessages);
                                }
                                if (vList.FunctionName == "OnSendToServer")
                                {
                                    string ip = (string)vList.functionArgs[4];

                                    if (ip.Contains("|"))
                                        ip = ip.Substring(0, ip.IndexOf("|"));

                                    int port = (int)vList.functionArgs[1];
                                    userID = (int)vList.functionArgs[3];
                                    token = (int)vList.functionArgs[2];
                                    lmode = (int)vList.functionArgs[5];
                                    Growtopia_IP = ip;
                                    Growtopia_Port = port;
                                    ConnectCurrent();
                                    this.BeginInvoke(new MethodInvoker(delegate ()
                                    {
                                        statss.Text = "Status: Connected to game.";
                                    }));
                                }
                                if (vList.FunctionName == "OnConsoleMessage")//   ????                                         L       
                                {
                                    string foundedmessage = Encoding.ASCII.GetString(packet.Skip(4).ToArray());
                                    string lol = Regex.Replace(foundedmessage, "\\x60[a-zA-Z0-9!@#$%^&*()_+\\-=\\[\\]\\{};':\"\\\\|,.<>\\/?]", "");
                                    Console.WriteLine(lol.Replace("", "").Replace("?", "").Replace("", "").Replace("", "").Replace(" ", "").Replace("                                                   ", ""));

                                    if (foundedmessage.IndexOf("clock") != -1)
                                    {

                                        packetSender.SendPacket(2, "action|input\n|text|Turkey Clock:" + DateTime.Now.ToString("h:mm:ss tt"), g_Peer);
                                    }

                                    if (foundedmessage.IndexOf("quit") != -1)
                                    {
                                            packetSender.SendPacket(2, "action|input\n|text|respawned", g_Peer);
                                            packetSender.SendPacket(2, "action|respawn", g_Peer);
                                    }

                                    if (foundedmessage.IndexOf("respawn") != -1)
                                    {
                                        packetSender.SendPacket(2, "action|input\n|text|ben oluyom", g_Peer);
                                        packetSender.SendPacket(3, "action|respawn", g_Peer);
                                    }
                                    if (foundedmessage.IndexOf("dance") != -1)
                                    {
                                        //BatuhanGG player_chat=test
                                        packetSender.SendPacket(2, "action|input\n|text|/dance", g_Peer);
                                    }
                                    Regex a = new Regex("[0-9]");
                                    if (foundedmessage.IndexOf("drop") != -1)
                                    {

                                        string id = foundedmessage;
                                        id = string.Join("", id.ToCharArray().Where(Char.IsDigit)).Replace("006","").Replace("046","");
                                        char last = id[id.Length - 1];
                                        string lastd = last.ToString();
                                       
                                        packetSender.SendPacket(2, "action|input\n|text|" + "If have item of " + lastd.ToString(), g_Peer);
                                        packetSender.SendPacket(2, "action|drop\nitemID|" + lastd.ToString() + "|\n", g_Peer);
                                        string str = "action|dialog_return\n" +
    "dialog_name|drop_item\n" +
    "itemID|" + lastd.ToString() + "|\n" +
    "count|" + "1" + "\n";
                                        packetSender.SendPacket(2, str, g_Peer);
                                    }

                                    if (foundedmessage.IndexOf("inven") != -1)
                                    {
                                        Inventory inventory = player.inventory;
                                        if (inventory.items == null)
                                        {
                                            Console.WriteLine("inventory.items was null!");
                                        }
                                        foreach (InventoryItem item in inventory.items)
                                        {
                                            Console.WriteLine(item + " 1111111111111111111111111111111111");
                                        }
                                    }
                                }
                            }
                        }
              
                            break;

                    case (byte)NetTypes.NetMessages.TRACK:
                        {
                            Console.WriteLine("TRACKOC\n"+Encoding.ASCII.GetString(packet.Skip(4).ToArray()));
                            // timer2.Start();

                            #region ss
                            if(Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("Level") != -1)
                            {
                                zdz++;
                                lvls += Encoding.ASCII.GetString(packet.Skip(4).ToArray()) + "\n";
                            }
                            if (Encoding.ASCII.GetString(packet.Skip(4).ToArray()).IndexOf("Worldlock") != -1)
                            {
                                zdz++;
                                wrold += Encoding.ASCII.GetString(packet.Skip(4).ToArray()) + "\n";
                            }
                            /*if(zdz ==2)
                            {
                                g_Peer.Disconnect(0);
                                all += lvls +"\n" +wrold;
                                //start();
                                zdz = 0;
                            }*/

                            #endregion

                            Growtopia_Port = Growtopia_Master_Port;
                            Growtopia_IP = Growtopia_Master_IP;
                            packetSender.SendPacket(2, "action|enter_game", g_Peer);
                            this.BeginInvoke(new MethodInvoker(delegate ()
                            {
                                statss.Text = "Status: Packet send.";

                            }));
                            break;
                        }
                    case (byte)NetTypes.PacketTypes.INVENTORY_STATE:
                       
                        Console.WriteLine("inventory systems");
                        break;
                    default:
                            Console.WriteLine("default [type"+packet[0]+" ]:"+Encoding.ASCII.GetString(packet.Skip(4).ToArray()));
                        break;
                }
            }
            catch
            {

            }
        }
        String getLineBySubstring(String myInput, String mySubstring)
        {
            string[] lines = myInput.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
                if (line.StartsWith(mySubstring))
                    return line;
            return "NaN";
        }
        private void Peer_OnDisconnect_Client(object sender, uint e)
        {
            this.BeginInvoke(new MethodInvoker(delegate ()
            {
                statss.Text = "Status: Checked.";
                guna2Button1.Enabled = true;
                guna2TextBox1.Enabled = true;
                guna2TextBox2.Enabled = true;
            }));
            Console.WriteLine("[ACCOUNT-CHECKER] Disconnected from GT Server(s)!");
            all += lvls + "\n" + wrold;
            start();
        }
        private void Client_OnConnect(object sender, ENetConnectEventArgs e)
        {
            e.Peer.OnReceive += Peer_OnReceive_Client;
            e.Peer.OnDisconnect += Peer_OnDisconnect_Client;
            e.Peer.PingInterval(1000);
            e.Peer.Timeout(1000, 9000, 13000);

            Console.WriteLine("[ACCOUNT-CHECKER] Successfully connected to GT Server(s)!");
        }
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if(guna2TextBox1.Text.Length < 3 || guna2TextBox2.Text.Length <3)
            {
                MessageBox.Show("Cannot be less account name than 3!");
                return;
            }
            guna2Button1.Enabled = false;
            guna2TextBox1.Enabled = false;
            guna2TextBox2.Enabled = false;
            ManagedENet.Startup();

            g_Client = new ENetHost(1, 2);
            g_Client.OnConnect += Client_OnConnect;
            g_Client.ChecksumWithCRC32();
            g_Client.CompressWithRangeCoder();
            g_Client.StartServiceThread();
            g_Peer = g_Client.Connect(new System.Net.IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 2, 0);
            this.BeginInvoke(new MethodInvoker(delegate ()
            {
                statss.Text = "Status: Connected gt.";

            }));
        }


        private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("I think you don't allow your gmail account for less secure apps.","Solution.",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            Process.Start("https://myaccount.google.com/lesssecureapps");
        }



        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Gmail method is unsafe, i recommend to use discord webhook receiver.","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            /*if(allofinone.Contains("Level") && allofinone.Contains("WorldLock_Balance"))
            {
                timer2.Stop();
                if (g_Peer.State == ENetPeerState.Connected)
                {
                    g_Peer.Disconnect(0);
                }

                var sw = new StringWriter();
                Console.SetOut(sw);
                Console.SetError(sw);
                string result = sw.ToString();
                MessageBox.Show(result);

            }*/
        }

        private void metroTabPage2_Click(object sender, EventArgs e)
        {

        }
    }
}
