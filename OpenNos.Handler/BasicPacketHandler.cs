﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.WebApi.Reference;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OpenNos.Handler
{
    public class BasicPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public BasicPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session => _session;

        #endregion

        #region Methods

        /// <summary>
        /// mJoinPacket packet
        /// </summary>
        /// <param name="mJoinPacket"></param>
        public void JoinMiniland(MJoinPacket mJoinPacket)
        {
            ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(mJoinPacket.CharacterId);
            if (sess?.Character != null)
            {
                ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, sess.Character.MinilandId, 5, 8);
            }
        }

        /// <summary>
        /// gop packet
        /// </summary>
        /// <param name="characterOptionPacket"></param>
        public void CharacterOptionChange(CharacterOptionPacket characterOptionPacket)
        {
            switch (characterOptionPacket.Option)
            {
                case CharacterOption.BuffBlocked:
                    Session.Character.BuffBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.BuffBlocked ? "BUFF_BLOCKED" : "BUFF_UNLOCKED"), 0));
                    break;

                case CharacterOption.EmoticonsBlocked:
                    Session.Character.EmoticonsBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.EmoticonsBlocked ? "EMO_BLOCKED" : "EMO_UNLOCKED"), 0));
                    break;

                case CharacterOption.ExchangeBlocked:
                    Session.Character.ExchangeBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.ExchangeBlocked ? "EXCHANGE_BLOCKED" : "EXCHANGE_UNLOCKED"), 0));
                    break;

                case CharacterOption.FriendRequestBlocked:
                    Session.Character.FriendRequestBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.FriendRequestBlocked ? "FRIEND_REQ_BLOCKED" : "FRIEND_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupRequestBlocked:
                    Session.Character.GroupRequestBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.GroupRequestBlocked ? "GROUP_REQ_BLOCKED" : "GROUP_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.HeroChatBlocked:
                    Session.Character.HeroChatBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.HeroChatBlocked ? "HERO_CHAT_BLOCKED" : "HERO_CHAT_UNLOCKED"), 0));
                    break;

                case CharacterOption.HpBlocked:
                    Session.Character.HpBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.HpBlocked ? "HP_BLOCKED" : "HP_UNLOCKED"), 0));
                    break;

                case CharacterOption.MinilandInviteBlocked:
                    Session.Character.MinilandInviteBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.MinilandInviteBlocked ? "MINI_INV_BLOCKED" : "MINI_INV_UNLOCKED"), 0));
                    break;

                case CharacterOption.MouseAimLock:
                    Session.Character.MouseAimLock = characterOptionPacket.IsActive;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.MouseAimLock ? "MOUSE_LOCKED" : "MOUSE_UNLOCKED"), 0));
                    break;

                case CharacterOption.QuickGetUp:
                    Session.Character.QuickGetUp = characterOptionPacket.IsActive;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.QuickGetUp ? "QUICK_GET_UP_ENABLED" : "QUICK_GET_UP_DISABLED"), 0));
                    break;

                case CharacterOption.WhisperBlocked:
                    Session.Character.WhisperBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.WhisperBlocked ? "WHISPER_BLOCKED" : "WHISPER_UNLOCKED"), 0));
                    break;

                case CharacterOption.FamilyRequestBlocked:
                    Session.Character.FamilyRequestBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.FamilyRequestBlocked ? "FAMILY_REQ_LOCKED" : "FAMILY_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupSharing:
                    Group grp = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup(Session.Character.CharacterId));
                    if (grp == null)
                    {
                        return;
                    }
                    if (grp.Characters.ElementAt(0) != Session)
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_MASTER"), 0));
                        return;
                    }
                    if (characterOptionPacket.IsActive == false)
                    {
                        Group group = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(Session.Character.CharacterId));
                        if (group != null)
                        {
                            group.SharingMode = 1;
                        }
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING"), 0), ReceiverType.Group);
                    }
                    else
                    {
                        Group group = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(Session.Character.CharacterId));
                        if (group != null)
                        {
                            group.SharingMode = 0;
                        }
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING_BY_ORDER"), 0), ReceiverType.Group);
                    }
                    break;
            }
            Session.SendPacket(Session.Character.GenerateStat());
        }

        [Packet("compl")]
        public void Compliment(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] complimentPacket = packet.Split(' ');
            long complimentedCharacterId;
            if (long.TryParse(complimentPacket[3], out complimentedCharacterId))
            {
                if (Session.Character.Level >= 30)
                {
                    if (Session.Character.LastLogin.AddMinutes(60) <= DateTime.Now)
                    {
                        if (Session.Account.LastCompliment.Date.AddDays(1) <= DateTime.Now.Date)
                        {
                            short? compliment = ServerManager.Instance.GetProperty<short?>(complimentedCharacterId, nameof(Character.Compliment));
                            compliment++;
                            ServerManager.Instance.SetProperty(complimentedCharacterId, nameof(Character.Compliment), compliment);
                            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_GIVEN"), ServerManager.Instance.GetProperty<string>(complimentedCharacterId, nameof(Character.Name))), 12));
                            Session.Account.LastCompliment = DateTime.Now;

                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_RECEIVED"), Session.Character.Name), 12), ReceiverType.OnlySomeone, complimentPacket[1].Substring(1));
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_COOLDOWN"), 11));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_LOGIN_COOLDOWN"), (Session.Character.LastLogin.AddMinutes(60) - DateTime.Now).Minutes), 11));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_NOT_MINLVL"), 11));
                }
            }
        }

        /// <summary>
        /// dir packet
        /// </summary>
        /// <param name="directionPacket"></param>
        public void Dir(DirectionPacket directionPacket)
        {
            if (directionPacket.CharacterId == Session.Character.CharacterId)
            {
                Session.Character.Direction = directionPacket.Direction;
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateDir());
            }
        }

        /// <summary>
        /// c_blist cbListPacket
        /// </summary>
        /// <param name="cbListPacket"></param>
        public void RefreshBazarList(CBListPacket cbListPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            Session.SendPacket(Session.Character.GenerateRCBList(cbListPacket));
        }

        /// <summary>
        /// c_slist csListPacket
        /// </summary>
        /// <param name="csListPacket"></param>
        public void RefreshPersonalBazarList(CSListPacket csListPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            Session.SendPacket(Session.Character.GenerateRCSList(csListPacket));
        }

        /// <summary>
        /// c_skill cSkillPacket
        /// </summary>
        /// <param name="cSkillPacket"></param>
        public void OpenBazaar(CSkillPacket cSkillPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            StaticBonusDTO medal = Session.Character.StaticBonusList.FirstOrDefault(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
            if (medal != null)
            {
                byte Medal = medal.StaticBonusType == StaticBonusType.BazaarMedalGold ? (byte)MedalType.Gold : (byte)MedalType.Silver;
                int Time = (int)(medal.DateEnd - DateTime.Now).TotalHours;
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOTICE_BAZAAR"), 0));
                Session.SendPacket($"wopen 32 {Medal} {Time}");
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("INFO_BAZAAR")));
            }
        }

        /// <summary>
        /// c_buy cBuyPacket
        /// </summary>
        /// <param name="cBuyPacket"></param>
        public void BuyBazaar(CBuyPacket cBuyPacket)
        {
            BazaarItemDTO bz = DAOFactory.BazaarItemDAO.LoadAll().FirstOrDefault(s => s.BazaarItemId == cBuyPacket.BazaarId);
            if (bz != null && cBuyPacket.Amount > 0)
            {
                long price = cBuyPacket.Amount * bz.Price;

                if (Session.Character.Gold >= price)
                {
                    BazaarItemLink bzcree = new BazaarItemLink { BazaarItem = bz };
                    if (DAOFactory.CharacterDAO.LoadById(bz.SellerId) != null)
                    {
                        bzcree.Owner = DAOFactory.CharacterDAO.LoadById(bz.SellerId)?.Name;
                        bzcree.Item = (ItemInstance)DAOFactory.IteminstanceDAO.LoadById(bz.ItemInstanceId);
                    }
                    if (cBuyPacket.Amount <= bzcree.Item.Amount)
                    {

                        if (!Session.Character.Inventory.CanAddItem(bzcree.Item.ItemVNum))
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                            return;
                        }

                        if (bzcree.Item != null)
                        {
                            if (bz.IsPackage && cBuyPacket.Amount != bz.Amount)
                            {
                                return;
                            }
                            ItemInstanceDTO bzitemdto = DAOFactory.IteminstanceDAO.LoadById(bzcree.BazaarItem.ItemInstanceId);
                            if (bzitemdto.Amount < cBuyPacket.Amount)
                            {
                                return;
                            }
                            bzitemdto.Amount -= cBuyPacket.Amount;
                            Session.Character.Gold -= price;
                            Session.SendPacket(Session.Character.GenerateGold());
                            DAOFactory.IteminstanceDAO.InsertOrUpdate(bzitemdto);
                            ServerManager.Instance.BazaarRefresh(bzcree.BazaarItem.BazaarItemId);
                            Session.SendPacket($"rc_buy 1 {bzcree.Item.Item.VNum} {bzcree.Owner} {cBuyPacket.Amount} {cBuyPacket.Price} 0 0 0");
                            ItemInstance newBz = bzcree.Item.DeepCopy();
                            newBz.Id = Guid.NewGuid();
                            newBz.Amount = cBuyPacket.Amount;
                            newBz.Type = newBz.Item.Type;

                            ItemInstance newInv = Session.Character.Inventory.AddToInventory(newBz);
                            if (newInv != null)
                            {
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                                Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: { bzcree.Item.Item.Name} x {cBuyPacket.Amount}", 10));
                            }

                        }

                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateModal(Language.Instance.GetMessageFromKey("STATE_CHANGED"), 1));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                    Session.SendPacket(Session.Character.GenerateModal(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 1));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateModal(Language.Instance.GetMessageFromKey("STATE_CHANGED"), 1));
            }

        }

        /// <summary>
        /// c_scalc cScalcPacket
        /// </summary>
        /// <param name="cScalcPacket"></param>
        public void GetBazaar(CScalcPacket cScalcPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            BazaarItemDTO bz = DAOFactory.BazaarItemDAO.LoadAll().FirstOrDefault(s => s.BazaarItemId == cScalcPacket.BazaarId);
            if (bz != null)
            {
                ItemInstance Item = (ItemInstance)DAOFactory.IteminstanceDAO.LoadById(bz.ItemInstanceId);
                if (Item == null)
                    return;
                int soldedamount = bz.Amount - Item.Amount;
                long taxes = bz.MedalUsed ? 0 : (long)(bz.Price * 0.10 * soldedamount);
                long price = bz.Price * soldedamount - taxes;
                if (Session.Character.Gold + price <= 1000000000)
                {
                    Session.Character.Gold += price;
                    Session.SendPacket(Session.Character.GenerateGold());
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REMOVE_FROM_BAZAAR"), price), 10));
                    if (Item.Amount != 0)
                    {
                        ItemInstance newBz = Item.DeepCopy();
                        newBz.Id = Guid.NewGuid();
                        newBz.Type = newBz.Item.Type;

                        ItemInstance newInv = Session.Character.Inventory.AddToInventory(newBz);
                        if (newInv != null)
                        {
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                        }
                    }
                    Session.SendPacket($"rc_scalc 1 {bz.Price} {bz.Amount - Item.Amount} {bz.Amount} {taxes} {price + taxes}");

                    if (DAOFactory.BazaarItemDAO.LoadById(bz.BazaarItemId) != null)
                    {
                        DAOFactory.BazaarItemDAO.Delete(bz.BazaarItemId);
                    }

                    DAOFactory.IteminstanceDAO.Delete(Item.Id);

                    ServerManager.Instance.BazaarRefresh(bz.BazaarItemId);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                    Session.SendPacket($"rc_scalc 1 {bz.Price} 0 {bz.Amount} 0 0");
                }
            }
            else
            {
                Session.SendPacket($"rc_scalc 1 0 0 0 0 0");
            }
        }

        /// <summary>
        /// c_reg packet
        /// </summary>
        /// <param name="cRegPacket"></param>
        public void SellBazaar(CRegPacket cRegPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            StaticBonusDTO medal = Session.Character.StaticBonusList.FirstOrDefault(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);

            long price = cRegPacket.Price * cRegPacket.Amount;
            long taxmax = price > 100000 ? price / 200 : 500;
            long taxmin = price >= 4000 ? (60 + (price - 4000) / 2000 * 30 > 10000 ? 10000 : 60 + (price - 4000) / 2000 * 30) : 50;
            long tax = medal == null ? taxmax : taxmin;
            if (Session.Character.Gold < tax || cRegPacket.Amount <= 0 || Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.ExchangeList.Any() || Session.Character.IsShopping)
            {
                return;
            }
            ItemInstance it = Session.Character.Inventory.LoadBySlotAndType(cRegPacket.Slot, cRegPacket.Inventory == 4 ? 0 : (InventoryType)cRegPacket.Inventory);
            if (it == null || !it.Item.IsSoldable || it.IsBound)
            {
                return;
            }
            if (Session.Character.Inventory.CountItemInAnInventory(InventoryType.Bazaar) > 10 * (medal == null ? 1 : 10))
            {
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LIMIT_EXCEEDED"), 0));
                return;
            }
            if (price >= (medal == null ? 1000000 : 1000000000))
            {
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("PRICE_EXCEEDED"), 0));
                return;
            }
            ItemInstance bazar = Session.Character.Inventory.AddIntoBazaarInventory(cRegPacket.Inventory == 4 ? 0 : (InventoryType)cRegPacket.Inventory, cRegPacket.Slot, cRegPacket.Amount);
            if (bazar == null)
            {
                return;
            }
            short duration;
            switch (cRegPacket.Durability)
            {
                case 1:
                    duration = 24;
                    break;
                case 2:
                    duration = 168;
                    break;
                case 3:
                    duration = 360;
                    break;
                case 4:
                    duration = 720;
                    break;
                default:
                    return;
            }

            DAOFactory.IteminstanceDAO.InsertOrUpdate(bazar);

            BazaarItemDTO bz = new BazaarItemDTO
            {
                Amount = bazar.Amount,
                DateStart = DateTime.Now,
                Duration = duration,
                IsPackage = cRegPacket.IsPackage != 0,
                MedalUsed = medal != null,
                Price = cRegPacket.Price,
                SellerId = Session.Character.CharacterId,
                ItemInstanceId = bazar.Id,
            };


            DAOFactory.BazaarItemDAO.InsertOrUpdate(ref bz);
            ServerManager.Instance.BazaarRefresh(bz.BazaarItemId);

            Session.Character.Gold -= tax;
            Session.SendPacket(Session.Character.GenerateGold());

            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("OBJECT_IN_BAZAAR"), 10));
            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("OBJECT_IN_BAZAAR"), 0));

            Session.SendPacket("rc_reg 1");

        }

        [Packet("pcl")]
        public void GetGift(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                int id;
                if (!int.TryParse(packetsplit[3], out id))
                {
                    return;
                }

                if (Session.Character.MailList.ContainsKey(id))
                {
                    MailDTO mail = Session.Character.MailList[id];
                    if (packetsplit[2] == "4" && mail.AttachmentVNum != null)
                    {
                        if (Session.Character.Inventory.CanAddItem((short)mail.AttachmentVNum))
                        {
                            ItemInstance newInv = Session.Character.Inventory.AddNewToInventory((short)mail.AttachmentVNum, mail.AttachmentAmount);

                            if (newInv != null)
                            {
                                newInv.Upgrade = mail.AttachmentUpgrade;
                                newInv.Rare = (sbyte)mail.AttachmentRarity;
                                if (newInv.Rare != 0)
                                {
                                    WearableInstance wearable = newInv as WearableInstance;
                                    wearable?.SetRarityPoint();
                                }
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                                Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_GIFTED")}: {newInv.Item.Name} x {mail.AttachmentAmount}", 12));

                                if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                                {
                                    DAOFactory.MailDAO.DeleteById(mail.MailId);
                                }
                                Session.SendPacket($"parcel 2 1 {packetsplit[3]}");
                                if (Session.Character.MailList.ContainsKey(id))
                                {
                                    Session.Character.MailList.Remove(id);
                                }
                            }
                        }
                        else
                        {
                            Session.SendPacket("parcel 5 1 0");
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                        }
                    }
                    else if (packetsplit[2] == "5")
                    {
                        Session.SendPacket($"parcel 7 1 {packetsplit[3]}");

                        if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                        {
                            DAOFactory.MailDAO.DeleteById(mail.MailId);
                        }
                        if (Session.Character.MailList.ContainsKey(id))
                        {
                            Session.Character.MailList.Remove(id);
                        }
                    }
                }
            }
        }

        [Packet("ncif")]
        public void GetNamedCharacterInformation(string packet)
        {
            string[] characterInformationPacket = packet.Split(' ');
            if (characterInformationPacket[2] == "1")
            {
                long charId;
                if (long.TryParse(characterInformationPacket[3], out charId))
                {
                    ServerManager.Instance.RequireBroadcastFromUser(Session, charId, "GenerateStatInfo");
                }
            }
            if (characterInformationPacket[2] == "2" && Session.HasCurrentMapInstance)
            {
                foreach (MapNpc npc in Session.CurrentMapInstance.Npcs)
                {
                    int mapMonsterId;
                    if (int.TryParse(characterInformationPacket[3], out mapMonsterId))
                    {
                        if (npc.MapNpcId == mapMonsterId)
                        {
                            NpcMonster npcinfo = ServerManager.GetNpc(npc.NpcVNum);
                            if (npcinfo == null)
                            {
                                return;
                            }
                            Session.SendPacket($"st 2 {characterInformationPacket[3]} {npcinfo.Level} {npcinfo.HeroLevel} 100 100 50000 50000");
                        }
                    }
                }
            }
            if (characterInformationPacket[2] == "3" && Session.HasCurrentMapInstance)
            {
                foreach (MapMonster monster in Session.CurrentMapInstance.Monsters)
                {
                    int mapMonsterId;
                    if (int.TryParse(characterInformationPacket[3], out mapMonsterId))
                    {
                        if (monster.MapMonsterId == mapMonsterId)
                        {
                            NpcMonster monsterinfo = ServerManager.GetNpc(monster.MonsterVNum);
                            if (monsterinfo == null)
                            {
                                return;
                            }
                            Session.Character.LastMonsterId = monster.MapMonsterId;
                            Session.SendPacket($"st 3 {characterInformationPacket[3]} {monsterinfo.Level} {monsterinfo.HeroLevel} {(int)((float)monster.CurrentHp / (float)monster.Monster.MaxHP * 100)} {(int)((float)monster.CurrentMp / (float)monster.Monster.MaxMP * 100)} {monster.CurrentHp} {monster.CurrentMp}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// npinfo packet
        /// </summary>
        /// <param name="npinfoPacket"></param>
        public void GetStats(NpinfoPacket npinfoPacket)
        {
            Session.SendPacket(Session.Character.GenerateStatChar());
        }

        /// <summary>
        /// pjoin packet
        /// </summary>
        /// <param name="pjoinPacket"></param>
        public void GroupJoin(PJoinPacket pjoinPacket)
        {
            Logger.Debug("Joining group", Session.SessionId);

            if (pjoinPacket.RequestType.Equals(GroupRequestType.Requested) || pjoinPacket.RequestType.Equals(GroupRequestType.Invited))
            {
                if (pjoinPacket.CharacterId == 0)
                {
                    return;
                }
                if (ServerManager.Instance.IsCharactersGroupFull(pjoinPacket.CharacterId))
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                    return;
                }

                if (ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId) &&
                    ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId))
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_GROUP")));
                    return;
                }

                if (Session.Character.CharacterId != pjoinPacket.CharacterId)
                {
                    ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(pjoinPacket.CharacterId);
                    if (targetSession != null)
                    {

                        if (Session.Character.IsBlockedByCharacter(pjoinPacket.CharacterId))
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                            return;
                        }

                        if (targetSession.Character.GroupRequestBlocked)
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_BLOCKED"), 0));
                        }
                        else
                        {
                            // save sent group request to current character
                            Session.Character.GroupSentRequestCharacterIds.Add(targetSession.Character.CharacterId);

                            Session.SendPacket(Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("GROUP_REQUEST"), targetSession.Character.Name)));
                            targetSession.SendPacket(Session.Character.GenerateDialog($"#pjoin^3^{ Session.Character.CharacterId} #pjoin^4^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU"), Session.Character.Name)}"));
                        }
                    }
                }
            }
            else if (pjoinPacket.RequestType.Equals(GroupRequestType.Sharing))
            {
                if (Session.Character.Group != null)
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_SHARE_INFO")));
                    Session.Character.Group.Characters.Where(s => s.Character.CharacterId != Session.Character.CharacterId).ToList().ForEach(s => s.SendPacket(Session.Character.GenerateDialog($"#pjoin^6^{ Session.Character.CharacterId} #pjoin^7^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU_SHARE"), Session.Character.Name)}")));
                }

            }
        }

        [Packet("#pjoin")]
        public void GroupJoinValid(string packet)
        {
            Logger.Debug(packet, Session.SessionId);

            // serialization hack -> dialog answer packet isnt supported by PacketFactory atm
            PJoinPacket pjoinPacket = PacketFactory.Deserialize<PJoinPacket>(packet.Replace('^', ' ').Replace('#', ' '), true);
            bool createNewGroup = true;

            if (pjoinPacket != null)
            {
                if (pjoinPacket.CharacterId == 0)
                {
                    return;
                }

                ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(pjoinPacket.CharacterId);

                if (targetSession == null || !targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                {
                    // target session with character id does not exist or invalid request packet
                    return;
                }
                else
                {
                    targetSession.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);
                }

                // accepted, join the group
                if (pjoinPacket.RequestType.Equals(GroupRequestType.Accepted))
                {
                    if (ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId) &&
                        ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId))
                    {
                        // everyone is in group, return
                        return;
                    }

                    if (ServerManager.Instance.IsCharactersGroupFull(pjoinPacket.CharacterId)
                        || ServerManager.Instance.IsCharactersGroupFull(Session.Character.CharacterId))
                    {
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                        targetSession.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                        return;
                    }

                    // get group and add to group
                    if (ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId))
                    {
                        // target joins source
                        Group currentGroup = ServerManager.Instance.GetGroupByCharacterId(Session.Character.CharacterId);

                        if (currentGroup != null)
                        {
                            currentGroup.JoinGroup(targetSession);
                            targetSession.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOINED_GROUP"), 10));
                            createNewGroup = false;
                        }
                    }
                    else if (ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId))
                    {
                        // source joins target
                        Group currentGroup = ServerManager.Instance.GetGroupByCharacterId(pjoinPacket.CharacterId);

                        if (currentGroup != null)
                        {
                            currentGroup.JoinGroup(Session);
                            createNewGroup = false;
                        }
                    }

                    if (createNewGroup)
                    {
                        Group group = new Group();
                        group.JoinGroup(pjoinPacket.CharacterId);
                        Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("GROUP_JOIN"), targetSession.Character.Name), 10));
                        group.JoinGroup(Session.Character.CharacterId);
                        ServerManager.Instance.AddGroup(group);
                        targetSession.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_ADMIN")));

                        // set back reference to group
                        Session.Character.Group = group;
                        targetSession.Character.Group = group;
                    }

                    // player join group
                    ServerManager.Instance.UpdateGroup(pjoinPacket.CharacterId);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GeneratePidx());
                }
                else if (pjoinPacket.RequestType == GroupRequestType.Declined)
                {
                    targetSession.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REFUSED_GROUP_REQUEST"), Session.Character.Name), 10));
                }
                else if (pjoinPacket.RequestType == GroupRequestType.AcceptedShare)
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ACCEPTED_SHARE"), 0));
                    if (Session.Character.Group.IsMemberOfGroup(pjoinPacket.CharacterId))
                    {
                        Session.Character.SetReturnPoint(Session.Character.MapInstance.Map.MapId, targetSession.Character.PositionX, targetSession.Character.PositionY);
                        targetSession.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("CHANGED_SHARE"), targetSession.Character.Name), 0));
                    }
                }
                else if (pjoinPacket.RequestType == GroupRequestType.DeclinedShare)
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REFUSED_SHARE"), 0));
                }
            }
        }
        /// <summary>
        /// pleave packet
        /// </summary>
        /// <param name="pleavePacket"></param>
        public void GroupLeave(PLeavePacket pleavePacket)
        {
            ServerManager.Instance.GroupLeave(Session);
        }

        /// <summary>
        /// ; packet
        /// </summary>
        /// <param name="groupSayPacket"></param>
        public void GroupTalk(GroupSayPacket groupSayPacket)
        {
            ServerManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(groupSayPacket.Message, 3), ReceiverType.Group);
        }

        [Packet("btk")]
        public void FriendTalk(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length >= 4)
            {
                long characterId;
                if (long.TryParse(packetsplit[2], out characterId))
                {
                    string message = string.Empty;
                    for (int i = 3; i < packetsplit.Length; i++)
                    {
                        message += packetsplit[i] + " ";
                    }
                    if (message.Length > 60)
                    {
                        message = message.Substring(0, 60);
                    }

                    message = message.Trim();

                    ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                    if (otherSession != null)
                    {
                        // Yes, it has to be two spaces!
                        otherSession.SendPacket($"talk  {Session.Character.CharacterId} {message}");
                    }
                    else
                    {
                        //session is not on current server, check api if the target character is on another server
                        int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", $"talk  {Session.Character.CharacterId} {message}"
                                                                         , ServerManager.Instance.ChannelId, MessageType.PrivateChat, null, (long?)characterId).Result;
                        if (!sentChannelId.HasValue) //character is even offline on different world
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_OFFLINE")));
                        }
                    }
                }
            }
        }

        [Packet("fdel")]
        public void FriendDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                long characterId;
                if (long.TryParse(packetsplit[2], out characterId))
                {
                    Session.Character.DeleteRelation(characterId);
                    Session.SendPacket(Session.Character.GenerateFinit());
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_DELETED")));

                    ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                    if (otherSession != null)
                    {
                        otherSession.Character.DeleteRelation(Session.Character.CharacterId);
                        otherSession.SendPacket(otherSession.Character.GenerateFinit());
                    }
                    else
                    {
                        DAOFactory.CharacterRelationDAO.Delete(Session.Character.CharacterId, characterId);
                    }
                }
            }
        }

        [Packet("fins")]
        public void FriendAdd(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 4)
            {
                if (!Session.Character.IsFriendlistFull())
                {
                    long characterId;
                    if (long.TryParse(packetsplit[3], out characterId))
                    {
                        if (!Session.Character.IsFriendOfCharacter(characterId))
                        {
                            if (!Session.Character.IsBlockedByCharacter(characterId))
                            {
                                if (!Session.Character.IsBlockingCharacter(characterId))
                                {
                                    ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                                    if (otherSession != null)
                                    {
                                        otherSession.SendPacket($"dlg #fins^-1^{Session.Character.CharacterId} #fins^-99^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("FRIEND_ADD"), Session.Character.Name)}");
                                        Session.Character.FriendRequestCharacters.Add(characterId);
                                    }
                                }
                                else
                                {
                                    Session.SendPacket($"info {Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKING")}");
                                }
                            }
                            else
                            {
                                Session.SendPacket($"info {Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")}");
                            }
                        }
                        else
                        {
                            Session.SendPacket($"info {Language.Instance.GetMessageFromKey("ALREADY_FRIEND")}");
                        }
                    }
                }
                else
                {
                    Session.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_FULL")}");
                }
            }
        }

        [Packet("#fins")]
        public void FriendAddResponse(string packet)
        {
            string[] packetsplit = packet.Replace('^', ' ').Split(' ');
            if (packetsplit.Length == 4)
            {
                long characterId;
                if (long.TryParse(packetsplit[3], out characterId) && !Session.Character.IsFriendOfCharacter(characterId) && !Session.Character.IsBlockedByCharacter(characterId) && !Session.Character.IsBlockingCharacter(characterId))
                {
                    ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                    if (otherSession != null)
                    {
                        if (otherSession.Character.FriendRequestCharacters.Contains(Session.Character.CharacterId))
                        {
                            switch (packetsplit[2])
                            {
                                case "-1":
                                    Session.Character.AddRelation(characterId, CharacterRelationType.Friend);
                                    Session.SendPacket(Session.Character.GenerateFinit());
                                    otherSession.Character.AddRelation(Session.Character.CharacterId, CharacterRelationType.Friend);
                                    otherSession.SendPacket(otherSession.Character.GenerateFinit());
                                    Session.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_ADDED")}");
                                    otherSession.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_ADDED")}");
                                    break;

                                case "-99":
                                    otherSession.Character.DeleteRelation(Session.Character.CharacterId);
                                    otherSession.SendPacket(Language.Instance.GetMessageFromKey("FRIEND_REJECTED"));
                                    break;

                                default:
                                    if (Session.Character.IsFriendlistFull())
                                    {
                                        Session.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_FULL")}");
                                        otherSession.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_FULL")}");
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        [Packet("bldel")]
        public void BlacklistDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                long characterId;
                if (long.TryParse(packetsplit[2], out characterId))
                {
                    Session.Character.DeleteRelation(characterId);
                    Session.SendPacket(Session.Character.GenerateBlinit());
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_DELETED")));
                }
            }
        }

        [Packet("blins")]
        public void BlacklistAdd(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                long characterId;
                if (long.TryParse(packetsplit[2], out characterId))
                {
                    Session.Character.AddRelation(characterId, CharacterRelationType.Blocked);
                    Session.SendPacket(Session.Character.GenerateBlinit());
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_ADDED")));
                }
            }
        }

        [Packet("guri")]
        public void Guri(string packet)
        {
            string[] guriPacket = packet.Split(' ');
            if (guriPacket[2] == "10" && Convert.ToInt32(guriPacket[5]) >= 973 && Convert.ToInt32(guriPacket[5]) <= 999 && !Session.Character.EmoticonsBlocked)
            {
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateEff(Convert.ToInt32(guriPacket[5]) + 4099), ReceiverType.AllNoEmoBlocked);
            }
            if (guriPacket[2] == "2")
            {
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGuri(2, 1), Session.Character.PositionX, Session.Character.PositionY);
            }
            else if (guriPacket[2] == "4")
            {
                const int speakerVNum = 2173;

                // presentation message
                if (guriPacket[3] == "2")
                {
                    int presentationVNum = Session.Character.Inventory.CountItem(1117) > 0 ? 1117 : (Session.Character.Inventory.CountItem(9013) > 0 ? 9013 : -1);
                    if (presentationVNum != -1)
                    {
                        string message = string.Empty;

                        // message = $" ";
                        for (int i = 6; i < guriPacket.Length; i++)
                        {
                            message += guriPacket[i] + "^";
                        }
                        message = message.Substring(0, message.Length - 1); // Remove the last ^
                        message = message.Trim();
                        if (message.Length > 60)
                        {
                            message = message.Substring(0, 60);
                        }

                        Session.Character.Biography = message;
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("INTRODUCTION_SET"), 10));
                        Session.Character.Inventory.RemoveItemAmount(presentationVNum);
                    }
                }

                // Speaker
                if (guriPacket[3] == "3")
                {
                    if (Session.Character.Inventory.CountItem(speakerVNum) > 0)
                    {
                        string message = $"<{Language.Instance.GetMessageFromKey("SPEAKER")}> [{Session.Character.Name}]:";
                        for (int i = 6; i < guriPacket.Length; i++)
                        {
                            message += guriPacket[i] + " ";
                        }
                        if (message.Length > 120)
                        {
                            message = message.Substring(0, 120);
                        }

                        message = message.Trim();

                        if (Session.Character.IsMuted())
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SPEAKER_CANT_BE_USED"), 10));
                            return;
                        }
                        Session.Character.Inventory.RemoveItemAmount(speakerVNum);
                        ServerManager.Instance.Broadcast(Session.Character.GenerateSay(message, 13));
                    }
                }
            }
            else if (guriPacket[2] == "199" && guriPacket[3] == "1")
            {
                long charId;
                long.TryParse(guriPacket[4], out charId);
                if (!Session.Character.IsFriendOfCharacter(charId))
                {
                    Session.SendPacket(Language.Instance.GetMessageFromKey("CHARACTER_NOT_IN_FRIENDLIST"));
                    return;
                }
                Session.SendPacket(Session.Character.GenerateDelay(3000, 4, $"#guri^199^{charId}"));
            }
            else if (guriPacket[2] == "208" && guriPacket[3] == "0")
            {
                short pearlSlot;
                short mountSlot;
                if (short.TryParse(guriPacket[4], out pearlSlot) && short.TryParse(guriPacket[6], out mountSlot))
                {
                    ItemInstance mount = Session.Character.Inventory.LoadBySlotAndType<ItemInstance>(mountSlot, InventoryType.Main);
                    BoxInstance pearl = Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(pearlSlot, InventoryType.Equipment);
                    if (mount != null && pearl != null)
                    {
                        pearl.HoldingVNum = mount.ItemVNum;
                        Session.Character.Inventory.RemoveItemAmountFromInventory(1, mount.Id);
                    }
                }
            }
            else if (guriPacket[2] == "209" && guriPacket[3] == "0")
            {
                short pearlSlot;
                short mountSlot;
                if (short.TryParse(guriPacket[4], out pearlSlot) && short.TryParse(guriPacket[6], out mountSlot))
                {
                    WearableInstance fairy = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(mountSlot, InventoryType.Equipment);
                    BoxInstance pearl = Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(pearlSlot, InventoryType.Equipment);
                    if (fairy != null && pearl != null)
                    {
                        pearl.HoldingVNum = fairy.ItemVNum;
                        pearl.ElementRate = fairy.ElementRate;
                        Session.Character.Inventory.RemoveItemAmountFromInventory(1, fairy.Id);
                    }
                }
            }
            else if (guriPacket[2] == "203" && guriPacket[3] == "0")
            {
                // SP points initialization
                int[] listPotionResetVNums = { 1366, 1427, 5115, 9040 };
                int vnumToUse = -1;
                foreach (int vnum in listPotionResetVNums)
                {
                    if (Session.Character.Inventory.CountItem(vnum) > 0)
                    {
                        vnumToUse = vnum;
                    }
                }
                if (vnumToUse != -1)
                {
                    if (Session.Character.UseSp)
                    {
                        SpecialistInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                        if (specialistInstance != null)
                        {
                            specialistInstance.SlDamage = 0;
                            specialistInstance.SlDefence = 0;
                            specialistInstance.SlElement = 0;
                            specialistInstance.SlHP = 0;

                            specialistInstance.DamageMinimum = 0;
                            specialistInstance.DamageMaximum = 0;
                            specialistInstance.HitRate = 0;
                            specialistInstance.CriticalLuckRate = 0;
                            specialistInstance.CriticalRate = 0;
                            specialistInstance.DefenceDodge = 0;
                            specialistInstance.DistanceDefenceDodge = 0;
                            specialistInstance.ElementRate = 0;
                            specialistInstance.DarkResistance = 0;
                            specialistInstance.LightResistance = 0;
                            specialistInstance.FireResistance = 0;
                            specialistInstance.WaterResistance = 0;
                            specialistInstance.CriticalDodge = 0;
                            specialistInstance.CloseDefence = 0;
                            specialistInstance.DistanceDefence = 0;
                            specialistInstance.MagicDefence = 0;
                            specialistInstance.HP = 0;
                            specialistInstance.MP = 0;

                            Session.Character.Inventory.RemoveItemAmount(vnumToUse);
                            Session.Character.Inventory.DeleteFromSlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                            Session.Character.Inventory.AddToInventoryWithSlotAndType(specialistInstance, InventoryType.Wear, (byte)EquipmentType.Sp);
                            Session.SendPacket(Session.Character.GenerateCond());
                            Session.SendPacket(Session.Character.GenerateSlInfo(specialistInstance, 2));
                            Session.SendPacket(Session.Character.GenerateLev());
                            Session.SendPacket(Session.Character.GenerateStatChar());
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_RESET"), 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORMATION_NEEDED"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POINTS"), 10));
                }
            }
        }

        [Packet("#guri")]
        public void GuriAnswer(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            switch (packetsplit[2])
            {
                case "199":
                    short[] listWingOfFriendship = { 2160, 2312, 10048 };
                    short vnumToUse = -1;
                    foreach (short vnum in listWingOfFriendship)
                    {
                        if (Session.Character.Inventory.CountItem(vnum) > 0)
                        {
                            vnumToUse = vnum;
                        }
                    }
                    if (vnumToUse != -1)
                    {
                        long charId;
                        if (!long.TryParse(packetsplit[3], out charId))
                        {
                            return;
                        }
                        ClientSession session = ServerManager.Instance.GetSessionByCharacterId(charId);
                        if (session != null)
                        {
                            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseInstance)
                            {
                                if (Session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseInstance)
                                {
                                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                                    return;
                                }
                                short mapy = session.Character.PositionY;
                                short mapx = session.Character.PositionX;
                                short mapId = session.Character.MapInstance.Map.MapId;

                                ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, mapId, mapx, mapy);
                                Session.Character.Inventory.RemoveItemAmount(vnumToUse);
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_ON_INSTANCEMAP"), 0));
                            }
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_WINGS"), 10));
                    }
                    break;

                case "400":
                    if (packetsplit.Length > 3)
                    {
                        short MapNpcId;
                        if (!short.TryParse(packetsplit[3], out MapNpcId))
                        {
                            return;
                        }
                        if (!Session.HasCurrentMapInstance)
                        {
                            return;
                        }
                        MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(n => n.MapNpcId.Equals(MapNpcId));
                        if (npc != null)
                        {
                            NpcMonster mapobject = ServerManager.GetNpc(npc.NpcVNum);

                            int RateDrop = ServerManager.DropRate;
                            int delay = (int)Math.Round((3 + mapobject.RespawnTime / 1000d) * Session.Character.TimesUsed);
                            delay = delay > 11 ? 8 : delay;
                            if (Session.Character.LastMapObject.AddSeconds(delay) < DateTime.Now)
                            {
                                if (mapobject.Drops.Any(s => s.MonsterVNum != null))
                                {
                                    if (mapobject.VNumRequired > 10 && Session.Character.Inventory.CountItem(mapobject.VNumRequired) < mapobject.AmountRequired)
                                    {
                                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEM"), 0));
                                        return;
                                    }
                                }
                                Random random = new Random();
                                double randomAmount = ServerManager.RandomNumber() * random.NextDouble();
                                DropDTO drop = mapobject.Drops.FirstOrDefault(s => s.MonsterVNum == npc.NpcVNum);
                                if (drop != null)
                                {
                                    int dropChance = drop.DropChance;
                                    if (randomAmount <= (double)dropChance * RateDrop / 5000.000)
                                    {
                                        short vnum = drop.ItemVNum;
                                        ItemInstance newInv = Session.Character.Inventory.AddNewToInventory(vnum);
                                        Session.Character.LastMapObject = DateTime.Now;
                                        Session.Character.TimesUsed++;
                                        if (Session.Character.TimesUsed >= 4)
                                        {
                                            Session.Character.TimesUsed = 0;
                                        }
                                        if (newInv != null)
                                        {
                                            Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                                            Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), newInv.Item.Name), 0));
                                            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), newInv.Item.Name), 11));
                                        }
                                        else
                                        {
                                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                        }
                                    }
                                    else
                                    {
                                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("TRY_FAILED"), 0));
                                    }
                                }
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("TRY_FAILED_WAIT"), (int)(Session.Character.LastMapObject.AddSeconds(delay) - DateTime.Now).TotalSeconds), 0));
                            }
                        }
                    }
                    break;

                case "710":
                    if (packetsplit.Length > 5)
                    {
                        // MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(n =>
                        // n.MapNpcId.Equals(Convert.ToInt16(packetsplit[5]))); NpcMonster mapObject
                        // = ServerManager.GetNpc(npc.NpcVNum); teleport free
                    }
                    break;

                case "750":
                    if (System.Configuration.ConfigurationManager.AppSettings["Act4Mode"] == "Y")
                    {
                        Session.SendPacket("msg 0 You can not use this item now!");
                        break;
                    }

                    if (packetsplit.Length > 3)
                    {
                        short faction;
                        const short baseVnum = 1623;
                        if (short.TryParse(packetsplit[3], out faction))
                        {
                            if (Session.Character.Inventory.CountItem(baseVnum + faction) > 0)
                            {
                                Session.Character.Faction = faction;
                                Session.Character.Inventory.RemoveItemAmount(baseVnum + faction);

                                // Reset
                                Session.Character.Act4Kill = 0;
                                Session.Character.Act4Dead = 0;
                                Session.Character.Act4Points = 0;

                                Session.SendPacket("scr " + Session.Character.Act4Kill + " " + Session.Character.Act4Dead + " " + Session.Character.Act4Points + " " + Session.Character.Compliment + " 0 0");

                                Session.SendPacket(Session.Character.GenerateFaction());
                                Session.SendPacket(Session.Character.GenerateEff(4799 + faction));
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction}"), 0));
                            }
                        }
                    }
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_GURI"), GetType()));
                    break;
            }
        }

        [Packet("hero")]
        public void Hero(string packet)
        {
            if (DAOFactory.CharacterDAO.IsReputHero(Session.Character.CharacterId) >= 3)
            {
                string[] packetsplit = packet.Split(' ');
                string message = string.Empty;
                for (int i = 2; i < packetsplit.Length; i++)
                {
                    message += packetsplit[i] + " ";
                }
                message = message.Trim();

                ServerManager.Instance.Broadcast(Session, $"msg 5 [{Session.Character.Name}]:{message}", ReceiverType.AllNoHeroBlocked);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_HERO"), 11));
            }
        }

        [Packet("preq")]
        public void Preq(string packet)
        {
            Logger.Debug(packet, Session.SessionId);

            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
            double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
            if (!(timeSpanSinceLastPortal >= 4) || !Session.HasCurrentMapInstance)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                return;
            }
            foreach (Portal portal in Session.CurrentMapInstance.Portals.Concat(Session.Character.GetExtraPortal()))
            {
                if (Session.Character.PositionY >= portal.SourceY - 1 && Session.Character.PositionY <= portal.SourceY + 1
                    && Session.Character.PositionX >= portal.SourceX - 1 && Session.Character.PositionX <= portal.SourceX + 1)
                {
                    switch (portal.Type)
                    {
                        case (sbyte)PortalType.MapPortal:
                        case (sbyte)PortalType.TSNormal:
                        case (sbyte)PortalType.Open:
                        case (sbyte)PortalType.Miniland:
                        case (sbyte)PortalType.TSEnd:
                        case (sbyte)PortalType.End:
                        case (sbyte)PortalType.Effect:
                        case (sbyte)PortalType.ShopTeleport:
                            break;

                        default:
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                            return;
                    }

                    if (Session.Character.Hp < 1)
                        return;

                    if ((Session.Character.Faction == 1 && portal.DestinationMapId == 131)
                        || (Session.Character.Faction == 2 && portal.DestinationMapId == 130))
                    {
                        return;
                    }

                    ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                    Session.Character.LastPortal = currentRunningSeconds;

                    if (ServerManager.GetMapInstance(portal.SourceMapInstanceId).MapInstanceType != MapInstanceType.BaseInstance && ServerManager.GetMapInstance(portal.DestinationMapInstanceId).MapInstanceType == MapInstanceType.BaseInstance)
                    {
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
                    }
                    else
                    {
                        ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, portal.DestinationMapInstanceId, portal.DestinationX, portal.DestinationY);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Pulse packet
        /// </summary>
        /// <param name="pulsepacket"></param>
        public void Pulse(PulsePacket pulsepacket)
        {
            Session.Character.LastPulse += 60;
            if (pulsepacket.Tick != Session.Character.LastPulse)
            {
                Session.Disconnect();
            }
            Session.Character.DeleteTimeout();
        }

        [Packet("req_info")]
        public void ReqInfo(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2] == "5")
            {
                short npcVNum;
                if (short.TryParse(packetsplit[3], out npcVNum))
                {
                    NpcMonster npc = ServerManager.GetNpc(npcVNum);
                    if (npc != null)
                    {
                        Session.SendPacket(npc.GenerateEInfo());
                    }
                }
            }
            else
            {
                ServerManager.Instance.RequireBroadcastFromUser(Session, Convert.ToInt64(packetsplit[3]), "GenerateReqInfo");
            }
        }

        /// <summary>
        /// Rest packet
        /// </summary>
        /// <param name="sitpacket"></param>
        public void Rest(SitPacket sitpacket)
        {
            Session.Character.Rest();
        }

        [Packet("#revival")]
        public void Revive(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            if (packetsplit.Length > 2)
            {
                byte type;
                if (!byte.TryParse(packetsplit[2], out type))
                {
                    return;
                }
                if (Session.Character.Hp > 0)
                {
                    return;
                }
                switch (type)
                {
                    case 0:
                        const int seed = 1012;
                        if (Session.Character.Inventory.CountItem(seed) < 10 && Session.Character.Level > 20)
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POWER_SEED"), 0));
                            ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_SEED_SAY"), 0));
                        }
                        else
                        {
                            if (Session.Character.Level > 20)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("SEED_USED"), 10), 10));
                                Session.Character.Inventory.RemoveItemAmount(seed, 10);
                                Session.Character.Hp = (int)(Session.Character.HPLoad() / 2);
                                Session.Character.Mp = (int)(Session.Character.MPLoad() / 2);
                            }
                            else
                            {
                                Session.Character.Hp = (int)Session.Character.HPLoad();
                                Session.Character.Mp = (int)Session.Character.MPLoad();
                            }
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateTp());
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                            Session.SendPacket(Session.Character.GenerateStat());
                        }
                        break;

                    case 1:
                        ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                        break;
                    case 2:
                        if (Session.Character.Gold >= 100)
                        {
                            Session.Character.Hp = (int)Session.Character.HPLoad();
                            Session.Character.Mp = (int)Session.Character.MPLoad();
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateTp());
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                            Session.SendPacket(Session.Character.GenerateStat());
                            Session.Character.Gold -= 100;
                            Session.SendPacket(Session.Character.GenerateGold());
                            Session.Character.LastPVPRevive = DateTime.Now;
                        }
                        else
                        {
                            ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// say
        /// </summary>
        /// <param name="sayPacket"></param>
        public void Say(SayPacket sayPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            string message = sayPacket.Message;

            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 12));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 12));
                }
            }
            else
            {
                if (message == null)
                {
                    return;
                }
                string language = new CultureInfo(System.Configuration.ConfigurationManager.AppSettings["Language"]).EnglishName;
                if (message.Split(' ').Length > 3 && System.Configuration.ConfigurationManager.AppSettings["MainLanguageRequired"].ToLower() == "true" && !Language.Instance.CheckMessageIsCorrectLanguage(message))
                {
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("LANGUAGE_REQUIRED"), language), 2));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LANGUAGE_REQUIRED"), language), 11));
                }
                else
                {
                    if (Session.CurrentMapInstance != null && Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
                    {
                        // if you are dead then send %£$%£$ to all
                        if (Session.Character.Hp < 1)
                        {
                            // %£$%£$ to everyone
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay("%#$@#^&**\\!@#$#@%#$%", 1), ReceiverType.AllExceptMe);
                        }
                        else  // else
                        {
                            // %£$%£$ to enemies
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay("%#$@#^&**\\!@#$#@%#$%", 1), ReceiverType.AllExceptFactionAndMe);

                            // normal to faction
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(message.Trim(), 0), ReceiverType.OnlyFaction);
                        }
                    }
                    else
                    {
                        // normal to everyone
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(message.Trim(), 0), ReceiverType.AllExceptMe);
                    }
                }
            }
        }

        [Packet("pst")]
        public void SendMail(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            switch (packetsplit.Length)
            {
                case 10:
                    CharacterDTO Receiver = DAOFactory.CharacterDAO.LoadByName(packetsplit[7]);
                    if (Receiver != null)
                    {
                        WearableInstance headWearable = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);
                        byte color = headWearable != null && headWearable.Item.IsColored ? headWearable.Design : (byte)Session.Character.HairColor;
                        MailDTO mailcopy = new MailDTO
                        {
                            AttachmentAmount = 0,
                            IsOpened = false,
                            Date = DateTime.Now,
                            Title = packetsplit[8],
                            Message = packetsplit[9],
                            ReceiverId = Receiver.CharacterId,
                            SenderId = Session.Character.CharacterId,
                            IsSenderCopy = true,
                            SenderClass = Session.Character.Class,
                            SenderGender = Session.Character.Gender,
                            SenderHairColor = Enum.IsDefined(typeof(HairColorType), color) ? (HairColorType)color : 0,
                            SenderHairStyle = Session.Character.HairStyle,
                            EqPacket = Session.Character.GenerateEqListForPacket(),
                            SenderMorphId = Session.Character.Morph == 0 ? (short)-1 : (short)(Session.Character.Morph > short.MaxValue ? 0 : Session.Character.Morph)
                        };
                        MailDTO mail = new MailDTO
                        {
                            AttachmentAmount = 0,
                            IsOpened = false,
                            Date = DateTime.Now,
                            Title = packetsplit[8],
                            Message = packetsplit[9],
                            ReceiverId = Receiver.CharacterId,
                            SenderId = Session.Character.CharacterId,
                            IsSenderCopy = false,
                            SenderClass = Session.Character.Class,
                            SenderGender = Session.Character.Gender,
                            SenderHairColor = Enum.IsDefined(typeof(HairColorType), color) ? (HairColorType)color : 0,
                            SenderHairStyle = Session.Character.HairStyle,
                            EqPacket = Session.Character.GenerateEqListForPacket(),
                            SenderMorphId = Session.Character.Morph == 0 ? (short)-1 : (short)(Session.Character.Morph > short.MaxValue ? 0 : Session.Character.Morph)
                        };

                        DAOFactory.MailDAO.InsertOrUpdate(ref mailcopy);
                        DAOFactory.MailDAO.InsertOrUpdate(ref mail);

                        Session.Character.MailList.Add((Session.Character.MailList.Any() ? Session.Character.MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mailcopy);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAILED"), 11));
                        Session.SendPacket(Session.Character.GeneratePost(mailcopy, 2));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                    break;

                case 5:
                    int id;
                    byte type;
                    if (int.TryParse(packetsplit[4], out id) && byte.TryParse(packetsplit[3], out type))
                    {
                        if (packetsplit[2] == "3")
                        {
                            if (Session.Character.MailList.ContainsKey(id))
                            {
                                if (!Session.Character.MailList[id].IsOpened)
                                {
                                    Session.Character.MailList[id].IsOpened = true;
                                    MailDTO mailupdate = Session.Character.MailList[id];
                                    DAOFactory.MailDAO.InsertOrUpdate(ref mailupdate);
                                }
                                Session.SendPacket(Session.Character.GeneratePostMessage(Session.Character.MailList[id], type));
                            }
                        }
                        else if (packetsplit[2] == "2")
                        {
                            if (Session.Character.MailList.ContainsKey(id))
                            {
                                MailDTO mail = Session.Character.MailList[id];
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAIL_DELETED"), 11));
                                Session.SendPacket($"post 2 {type} {id}");
                                if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                                {
                                    DAOFactory.MailDAO.DeleteById(mail.MailId);
                                }
                                if (Session.Character.MailList.ContainsKey(id))
                                {
                                    Session.Character.MailList.Remove(id);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        [Packet("qset")]
        public void SetQuicklist(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 4)
            {
                short type, q1, q2, data1 = 0, data2 = 0;
                if (!short.TryParse(packetsplit[2], out type) || !short.TryParse(packetsplit[3], out q1) || !short.TryParse(packetsplit[4], out q2))
                {
                    return;
                }
                if (packetsplit.Length > 6)
                {
                    short.TryParse(packetsplit[5], out data1);
                    short.TryParse(packetsplit[6], out data2);
                }
                switch (type)
                {
                    case 0:
                    case 1:

                        // client says qset 0 1 3 2 6 answer -> qset 1 3 0.2.6.0
                        Session.Character.QuicklistEntries.RemoveAll(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                        Session.Character.QuicklistEntries.Add(new QuicklistEntryDTO
                        {
                            CharacterId = Session.Character.CharacterId,
                            Type = type,
                            Q1 = q1,
                            Q2 = q2,
                            Slot = data1,
                            Pos = data2,
                            Morph = Session.Character.UseSp ? (short)Session.Character.Morph : (short)0
                        });

                        Session.SendPacket($"qset {q1} {q2} {type}.{data1}.{data2}.0");
                        break;

                    case 2:

                        // DragDrop / Reorder qset type to1 to2 from1 from2 vars -> q1 q2 data1 data2
                        QuicklistEntryDTO qlFrom = Session.Character.QuicklistEntries.SingleOrDefault(n => n.Q1 == data1 && n.Q2 == data2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                        if (qlFrom != null)
                        {
                            QuicklistEntryDTO qlTo = Session.Character.QuicklistEntries.SingleOrDefault(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                            qlFrom.Q1 = q1;
                            qlFrom.Q2 = q2;

                            if (qlTo == null)
                            {
                                // Put 'from' to new position (datax)
                                Session.SendPacket($"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                                // old 'from' is now empty.
                                Session.SendPacket($"qset {data1} {data2} 7.7.-1.0");
                            }
                            else
                            {
                                // Put 'from' to new position (datax)
                                Session.SendPacket($"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                                // 'from' is now 'to' because they exchanged
                                qlTo.Q1 = data1;
                                qlTo.Q2 = data2;
                                Session.SendPacket($"qset {qlTo.Q1} {qlTo.Q2} {qlTo.Type}.{qlTo.Slot}.{qlTo.Pos}.0");
                            }
                        }

                        break;

                    case 3:

                        // Remove from Quicklist
                        Session.Character.QuicklistEntries.RemoveAll(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));
                        Session.SendPacket($"qset {q1} {q2} 7.7.-1.0");
                        break;

                    default:
                        return;
                }
            }
        }

        [Packet("game_start")]
        public void StartGame()
        {
            if (Session.IsOnMap || !Session.HasSelectedCharacter)
            {
                // character should have been selected in SelectCharacter
                return;
            }
            Session.CurrentMapInstance = Session.Character.MapInstance;
            if (System.Configuration.ConfigurationManager.AppSettings["SceneOnCreate"].ToLower() == "true" & DAOFactory.GeneralLogDAO.LoadByLogType("Connection", Session.Character.CharacterId).Count() == 1)
            {
                Session.SendPacket("scene 40");
            }
            if (System.Configuration.ConfigurationManager.AppSettings["WorldInformation"].ToLower() == "true")
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                string productVersion = assembly != null && assembly.Location != null ? FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion : "1337";
                Session.SendPacket(Session.Character.GenerateSay("----------[World Information]----------", 10));
                Session.SendPacket(Session.Character.GenerateSay($"OpenNos by OpenNos Team\nVersion : v{productVersion}", 11));
                Session.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));
            }
            Session.Character.LoadSpeed();
            Session.Character.LoadSkills();
            Session.SendPacket(Session.Character.GenerateTit());
            Session.SendPacket(Session.Character.GenerateSpPoint());
            Session.SendPacket("rsfi 1 1 0 9 0 9");

            if (Session.Character.Hp < 1)
            {
                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
            }
            else
            {
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
            }

            Session.SendPacket(Session.Character.GenerateSki());
            Session.SendPacket($"fd {Session.Character.Reput} 0 {(int)Session.Character.Dignity} {Math.Abs(Session.Character.GetDignityIco())}");
            Session.SendPacket(Session.Character.GenerateFd());
            Session.SendPacket("rage 0 250000");
            Session.SendPacket("rank_cool 0 0 18000");
            SpecialistInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(8, InventoryType.Wear);
            StaticBonusDTO medal = Session.Character.StaticBonusList.FirstOrDefault(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
            if (medal != null)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOGIN_MEDAL"), 12));
            }
            if (Session.Character.MapInstance.Map.MapId == 138)
            {
                Session.SendPacket("bc 0 0 0");
            }
            if (specialistInstance != null)
            {
                Session.SendPacket(Session.Character.GenerateSpPoint());
            }
            Session.SendPacket("scr " + Session.Character.Act4Kill + " " + Session.Character.Act4Dead + " " + Session.Character.Act4Points + " " + Session.Character.Compliment + " 0 0");

            if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
            {
                Session.SendPacket("fc " + Session.Character.Faction.ToString() + " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0");
                Session.SendPacket("guri 5 1 " + Session.Character.CharacterId + " " + (Session.Character.Faction + 2));

                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
            }

            for (int i = 0; i < 10; i++)
            {
                Session.SendPacket($"bn {i} {Language.Instance.GetMessageFromKey($"BN{i}")}");
            }
            Session.SendPacket(Session.Character.GenerateExts());
            Session.SendPacket($"mlinfo 3800 2000 100 0 0 10 0 {Language.Instance.GetMessageFromKey("WELCOME_MUSIC_INFO")} {Language.Instance.GetMessageFromKey("MINILAND_WELCOME_MESSAGE")}"); // 0 before 10 = visitors
            Session.SendPacket("p_clear");

            // sc_p pet sc_n nospartner Session.SendPacket("sc_p_stc 0"); // end pet and partner
            Session.SendPacket("pinit 0"); // clean party list

            Session.SendPacket("zzim");
            Session.SendPacket($"twk 2 {Session.Character.CharacterId} {Session.Account.Name} {Session.Character.Name} shtmxpdlfeoqkr");

            // qstlist target sqst bf
            Session.SendPacket("act6");
            Session.SendPacket(Session.Character.GenerateFaction());

            // sc_p pet again sc_n nospartner again
#pragma warning disable 618
            Session.Character.GenerateStartupInventory();
#pragma warning restore 618

            // mlobjlst - miniland object list
            Session.SendPacket(Session.Character.GenerateGold());
            Session.SendPackets(Session.Character.GenerateQuicklist());

            string clinit = "clinit";
            string flinit = "flinit";
            string kdlinit = "kdlinit";
            foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopComplimented())
            {
                clinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Compliment}|{character.Name}";
            }
            foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopReputation())
            {
                flinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Reput}|{character.Name}";
            }
            foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopPoints())
            {
                kdlinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Act4Points}|{character.Name}";
            }

            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());

            Session.SendPacket(Session.Character.GenerateFinit());
            Session.SendPacket(Session.Character.GenerateBlinit());
            Session.SendPacket(clinit);
            Session.SendPacket(flinit);
            Session.SendPacket(kdlinit);

            Session.Character.LastPVPRevive = DateTime.Now;

            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                Session.SendPacket(Session.Character.GenerateGInfo());
                Session.SendPackets(Session.Character.GetFamilyHistory());
                Session.SendPacket(Session.Character.GenerateFamilyMember());
                Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
                if (!string.IsNullOrWhiteSpace(Session.Character.Family.FamilyMessage))
                {
                    Session.SendPacket(Session.Character.GenerateInfo("--- Family Message ---\n" + Session.Character.Family.FamilyMessage));
                }
            }

            // finfo - friends info
            Session.SendPacket("p_clear");
            Session.Character.RefreshMail();
            Session.Character.LoadSentMail();
            Session.Character.DeleteTimeout();
        }

        public void Walk(WalkPacket walkPacket)
        {
            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
            double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
            int distance = Map.GetDistance(new MapCell() { X = Session.Character.PositionX, Y = Session.Character.PositionY }, new MapCell() { X = walkPacket.XCoordinate, Y = walkPacket.YCoordinate });

            if (!Session.CurrentMapInstance.Map.IsBlockedZone(walkPacket.XCoordinate, walkPacket.YCoordinate) && !Session.Character.IsChangingMapInstance && !Session.Character.HasShopOpened)
            {
                if ((Session.Character.Speed >= walkPacket.Speed || Session.Character.LastSpeedChange.AddSeconds(1) > DateTime.Now) && !(distance > 60 && timeSpanSinceLastPortal > 10))
                {
                    if (Session.Character.MapInstance.MapInstanceType == MapInstanceType.BaseInstance)
                    {
                        Session.Character.MapX = walkPacket.XCoordinate;
                        Session.Character.MapY = walkPacket.YCoordinate;
                    }
                    Session.Character.PositionX = walkPacket.XCoordinate;
                    Session.Character.PositionY = walkPacket.YCoordinate;

                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateMv());
                    Session.SendPacket(Session.Character.GenerateCond());
                    Session.Character.LastMove = DateTime.Now;
                }
                else
                {
                    Session.Disconnect();
                }
            }
        }
        /// <summary>
        /// / packet
        /// </summary>
        /// <param name="whisperPacket"></param>
        public void Whisper(WhisperPacket whisperPacket)
        {
            try
            {
                string characterName = whisperPacket.Message.Split(' ')[whisperPacket.Message.StartsWith("GM ") ? 1 : 0];
                string message = string.Empty;
                string[] packetsplit = whisperPacket.Message.Split(' ');

                for (int i = packetsplit[0] == "GM" ? 2 : 1; i < packetsplit.Length; i++)
                {
                    message += packetsplit[i] + " ";
                }
                if (message.Length > 60)
                {
                    message = message.Substring(0, 60);
                }

                message = message.Trim();

                CharacterDTO receiver = DAOFactory.CharacterDAO.LoadByName(characterName);
                if (receiver != null)
                {
                    if (Session.Character.IsBlockedByCharacter(receiver.CharacterId))
                    {
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                        return;
                    }
                }

                ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterName(characterName);
                if (targetSession == null)
                {
                    //session is not on current server, check api if the target character is on another server
                    int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", Session.Character.GenerateSpk(message, Session.Account.Authority == AuthorityType.Admin ? 15 : 5)
                                                                                                  , ServerManager.Instance.ChannelId, MessageType.Whisper, characterName, null).Result;
                    if (!sentChannelId.HasValue) //character is even offline on different world
                    {
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")));
                    }
                    else
                    {
                        //send message to sender
                        Session.SendPacket(Session.Character.GenerateSpk(message, 5));
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MESSAGE_SENT_TO_CHARACTER"), characterName, sentChannelId.Value), 11));
                    }

                    return;
                }

                if (packetsplit[0] == "GM" && targetSession.Account.Authority != AuthorityType.Admin)
                {
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("USER_IS_NOT_AN_ADMIN"), targetSession.Character.Name), 10));
                    return;
                }

                Session.SendPacket(Session.Character.GenerateSpk(message, 5));

                if (targetSession.Character.GmPvtBlock)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_CHAT_BLOCKED"), 10));
                    return;
                }

                if (!targetSession.Character.WhisperBlocked)
                {
                    ServerManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, Session.Account.Authority == AuthorityType.Admin ? 15 : 5), ReceiverType.OnlySomeone, characterName);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_WHISPER_BLOCKED"), 0));
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Whisper failed.", e);
            }
        }

        #endregion
    }
}