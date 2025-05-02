using BELibrary.Core.Utils;
using BELibrary.DbContext;
using BELibrary.Entity;
using HospitalManagement.Core.Dto;
using HospitalManagement.Models;
using HospitalManagement.signalr.hubs;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using VChatCore;

namespace HospitalManagement.Areas.Admin.Services
{
    public class ChatBoardService
    {
        protected readonly HospitalManagementDbContext context;
        protected IHubContext chatHub;

        public ChatBoardService(HospitalManagementDbContext context)
        {
            this.context = context;
            this.chatHub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
        }

        /// <summary>
        /// Danh sách lịch sử chat
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <returns>Danh sách lịch sử chat</returns>
        public List<GroupDto> GetHistory(Guid userSession)
        {
            // lấy danh sách nhóm chat
            List<GroupDto> groups = this.context.Groups
                     .Where(x => x.GroupUsers.Any(y => y.UserId == userSession))
                     .Select(x => new GroupDto()
                     {
                         Id = x.Id,
                         Name = x.Name,
                         Avatar = x.Avatar,
                         Type = x.Type,
                         LastActive = x.LastActive,
                         Users = x.GroupUsers.Select(y => new UserDto()
                         {
                             Id = y.User.Id,
                             FullName = y.User.FullName,
                             Avatar = y.User.LinkAvatar ?? Constants.AVATAR_DEFAULT
                         }).ToList()
                     }).ToList();

            foreach (var group in groups)
            {
                //Nếu nhóm chat có type = SINGLE (chat 1-1) => đổi tên nhóm chat thành tên người chat cùng
                if (group.Type == Constants.GroupType.SINGLE)
                {
                    var us = group.Users.FirstOrDefault(x => x.Id != userSession);
                    group.Name = us?.FullName;
                    group.Avatar = us?.Avatar;
                }

                // Lấy tin nhắn gần nhất để hiển thị
                group.LastMessage = this.context.Messages
                    .Where(x => !x.IsDeleted.HasValue || !x.IsDeleted.Value)
                    .Where(x => x.GroupId == group.Id)
                    .OrderByDescending(x => x.Created)
                    .Select(x => new MessageDto()
                    {
                        Created = x.Created,
                        CreatedBy = x.CreatedBy,
                        Content = x.Content,
                        GroupId = x.GroupId,
                        Type = x.Type,
                    }).FirstOrDefault();
            }

            return groups.OrderByDescending(x => x.LastActive)
                     .ToList();
        }

        /// <summary>
        /// Thông tin nhóm chat
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <param name="groupCode">Mã nhóm</param>
        /// <param name="contactCode">Người chat</param>
        /// <returns></returns>
        public UserChatResponse GetInfo(Guid userSession, Guid groupCode, Guid contactCode)
        {
            // Láy thông tin nhóm chat
            Group group = this.context.Groups.Include("GroupUsers").FirstOrDefault(x => x.Id == groupCode);

            // nếu mã nhóm k tồn tại => thuộc loại chat 1-1 (Tự quy định) => Trả về thông tin người chat cùng
            if (group == null)
            {
                return this.context.Accounts
                        .Where(x => x.Id == contactCode)
                        .OrderBy(x => x.FullName)
                        .Select(x => new UserChatResponse
                        {
                            IsGroup = false,
                            Id = x.Id,
                            Avatar = x.LinkAvatar ?? Constants.AVATAR_DEFAULT,
                            FullName = x.FullName,
                            Gender = x.Gender,
                            Phone = x.Phone
                        })
                        .FirstOrDefault();
            }
            else
            {
                // Nếu tồn tại nhóm chat + nhóm chat có type = SINGLE (Chat 1-1) => trả về thông tin người chat cùng
                if (group.Type.Equals(Constants.GroupType.SINGLE))
                {
                    var userId = group.GroupUsers.FirstOrDefault(x => x.UserId != userSession)?.UserId;
                    var acc = this.context.Accounts
                        .Where(x => x.Id == userId)
                        .OrderBy(x => x.FullName)
                        .FirstOrDefault();
                    return new UserChatResponse
                    {
                        IsGroup = false,
                        Id = acc.Id,
                        Avatar = acc.LinkAvatar ?? Constants.AVATAR_DEFAULT,
                        FullName = acc.FullName,
                        Gender = acc.Gender,
                        Phone = acc.Phone
                    };
                }
                else
                {
                    // Nếu tồn tại nhóm chat + nhóm chat nhiều người => trả về thông tin nhóm chat + thành viên trong nhóm
                    return new UserChatResponse
                    {
                        IsGroup = true,
                        Id = group.Id,
                        Avatar = group.Avatar,
                        Name = group.Name,
                        Type = group.Type,
                        Users = group.GroupUsers
                            .OrderBy(x => x.User.FullName)
                            .Select(x => new UserDto()
                            {
                                Id = x.User.Id,
                                FullName = x.User.FullName,
                                Avatar = x.User.LinkAvatar ?? Constants.AVATAR_DEFAULT
                            }).ToList()
                    };
                }
            }
        }

        /// <summary>
        /// Thêm mới nhóm chat
        /// </summary>
        /// <param name="userId">User hiện tại đang đăng nhập</param>
        /// <param name="group">Nhóm</param>
        public void AddGroup(Guid userId, GroupDto group)
        {
            DateTime dateNow = DateTime.Now;
            Group grp = new Group()
            {
                Id = Guid.NewGuid(),
                Name = group.Name,
                Created = dateNow,
                CreatedBy = userId,
                Type = Constants.GroupType.MULTI,
                LastActive = dateNow,
                Avatar = Constants.AVATAR_DEFAULT
            };

            List<GroupUser> groupUsers = group.Users.Select(x => new GroupUser()
            {
                Id = x.Id
            }).ToList();

            groupUsers.Add(new GroupUser()
            {
                UserId = userId
            });

            grp.GroupUsers = groupUsers;

            this.context.Groups.Add(grp);
            this.context.SaveChanges();
        }

        /// <summary>
        /// Cập nhật ảnh đại diện của nhóm chat
        /// </summary>
        /// <param name="group">Nhóm</param>
        /// <returns></returns>
        public GroupDto UpdateGroupAvatar(GroupDto group)
        {
            Group grp = this.context.Groups
                .FirstOrDefault(x => x.Id == group.Id);

            if (grp != null)
            {
                if (group.Avatar.Contains("data:image/png;base64,"))
                {
                    string pathAvatar = $"Resource/Avatar/{Guid.NewGuid():N}";
                    // string pathFile = Path.Combine(this.hostEnvironment.ContentRootPath, pathAvatar);
                    DataHelper.Base64ToImage(group.Avatar.Replace("data:image/png;base64,", ""), "pathFile");
                    grp.Avatar = group.Avatar = pathAvatar;
                }
                this.context.SaveChanges();
            }
            return group;
        }

        /// <summary>
        /// Xoá nhóm chat
        /// </summary>
        /// <param name="userId">Nhóm</param>
        /// <param name="groupId">Nhóm</param>
        /// <returns></returns>
        public void DeleteGroup(Guid userId, Guid groupId)
        {
            var group = this.context.Groups.FirstOrDefault(o => o.Id == groupId);
            if (group == null || group.CreatedBy != userId)
                throw new Exception("Nhóm không tồn tại hoặc bạn không có quyền xoá!");

            var groupUser = this.context.GroupUsers.Where(o => o.GroupId == groupId);
            var groupMessages = this.context.Messages.Where(o => o.GroupId == groupId);

            this.context.Messages.RemoveRange(groupMessages);
            this.context.GroupUsers.RemoveRange(groupUser);
            this.context.Groups.Remove(group);
            this.context.SaveChanges();

            this.EmitEventToClient();
        }

        /// <summary>
        /// Gửi tin nhắn
        /// </summary>
        /// <param name="userId">User hiện tại đang đăng nhập</param>
        /// <param name="groupId">Mã nhóm</param>
        /// <param name="message">Tin nhắn</param>
        public Guid SendMessage(Guid userId, Guid groupId, MessageDto message)
        {
            // Lấy thông tin nhóm chat
            Group grp = this.context.Groups.FirstOrDefault(x => x.Id == groupId);
            DateTime dateNow = DateTime.Now;

            // Nếu nhóm không tồn tại => cố gắng lấy thông tin nhóm đã từng chat giữa 2 người
            if (grp == null)
            {
                var grpCode = this.context.Groups
                    .Where(x => x.Type.Equals(Constants.GroupType.SINGLE))
                    .Where(x => x.GroupUsers.Any(z => z.UserId == userId) &&
                                x.GroupUsers.Any(y => y.UserId == message.SendTo))
                    .Select(x => x.Id)
                    .FirstOrDefault();

                grp = this.context.Groups.FirstOrDefault(x => x.Id == grpCode);
            }

            // Nếu nhóm vẫn không tồn tại => tạo nhóm chat mới có 2 thành viên
            if (grp == null)
            {
                Account sendTo = this.context.Accounts.FirstOrDefault(x => x.Id == message.SendTo);
                var groupEntityId = Guid.NewGuid();
                grp = new Group()
                {
                    Id = groupEntityId,
                    Name = sendTo.FullName,
                    Created = dateNow,
                    CreatedBy = userId,
                    Type = Constants.GroupType.SINGLE,
                    GroupUsers = new List<GroupUser>()
                        {
                            new GroupUser()
                            {
                                Id = Guid.NewGuid(),
                                GroupId = groupEntityId,
                                UserId = userId
                            },
                            new GroupUser()
                            {
                                Id = Guid.NewGuid(),
                                GroupId = groupEntityId,
                                UserId = sendTo.Id
                            }
                        }
                };
                this.context.Groups.Add(grp);
            }

            Message msg = new Message()
            {
                Id = Guid.NewGuid(),
                Content = message.Content,
                Created = dateNow,
                CreatedBy = userId,
                GroupId = grp.Id,
                Path = message.Path,
                Type = message.Type,
            };

            grp.LastActive = dateNow;

            this.context.Messages.Add(msg);
            this.context.SaveChanges();
            try
            {
                // Có thể tối ưu bằng cách chỉ gửi cho user trong nhóm chat
                this.chatHub.Clients.All.broadcastMessage(grp.Id, true);
                var b = this.chatHub.Groups;
                var a = ChatHub.GetClientCount();
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
            }
            return grp.Id;
        }

        /// <summary>
        /// Lấy danh sách tin nhắn từ nhóm
        /// </summary>
        /// <param name="userId">User hiện tại đang đăng nhập</param>
        /// <param name="groupCode">Mã nhóm</param>
        /// <returns>Danh sách tin nhắn</returns>
        public List<MessageDto> GetMessageByGroup(Guid userId, Guid groupCode)
        {
            return this.context.Messages
                    .Where(x => !x.IsDeleted.HasValue || !x.IsDeleted.Value)
                    .Where(x => x.GroupId == groupCode)
                    .Where(x => x.Group.GroupUsers.Any(y => y.UserId == userId))
                    .OrderBy(x => x.Created)
                    .Select(x => new MessageDto()
                    {
                        IsMe = x.CreatedBy == userId,
                        Created = x.Created,
                        Content = x.Content,
                        CreatedBy = x.CreatedBy,
                        GroupId = x.GroupId,
                        Id = x.Id,
                        Path = x.Path,
                        Type = x.Type,
                        IsPinned = x.IsPinned,
                        PinnedBy = x.PinnedBy,
                        PinnedDate = x.PinnedDate,
                        UserCreatedBy = new UserDto()
                        {
                            Id = x.UserCreatedBy.Id,
                            Avatar = x.UserCreatedBy.LinkAvatar ?? Constants.AVATAR_DEFAULT,
                            FullName = x.UserCreatedBy.FullName
                        }
                    }).ToList();
        }

        /// <summary>
        /// Lấy danh sách tin nhắn với người đã liên hệ
        /// </summary>
        /// <param name="userId">User hiện tại đang đăng nhập</param>
        /// <param name="contactId">Người nhắn cùng</param>
        /// <returns></returns>
        public List<MessageDto> GetMessageByContact(Guid userId, Guid contactId)
        {
            // Lấy mã nhóm đã từng nhắn tin giữa 2 người
            Guid groupCode = this.context.Groups
                    .Where(x => x.Type.Equals(Constants.GroupType.SINGLE))
                    .Where(x => x.GroupUsers.Any(z => z.UserId == userId &&
                                x.GroupUsers.Any(y => y.UserId == contactId)))
                    .Select(x => x.Id)
                    .FirstOrDefault();

            return this.context.Messages
                    .Where(x => !x.IsDeleted.HasValue || !x.IsDeleted.Value)
                    .Where(x => x.GroupId == groupCode)
                    .Where(x => x.Group.GroupUsers.Any(y => y.UserId == userId))
                    .OrderBy(x => x.Created)
                    .Select(x => new MessageDto()
                    {
                        IsMe = x.CreatedBy == userId,
                        Created = x.Created,
                        Content = x.Content,
                        CreatedBy = x.CreatedBy,
                        GroupId = x.GroupId,
                        Id = x.Id,
                        Path = x.Path,
                        Type = x.Type,
                        IsPinned = x.IsPinned,
                        UserCreatedBy = new UserDto()
                        {
                            Avatar = x.UserCreatedBy.LinkAvatar ?? Constants.AVATAR_DEFAULT,
                        }
                    }).ToList();
        }

        public void DeleteMessage(Guid userId, Guid messageId)
        {
            var message = this.context.Messages.FirstOrDefault(o => o.Id == messageId);
            if (message == null || message.IsDeleted == true)
                return;

            //  Only member in group can delete message
            var isMember = this.context.GroupUsers
                .Any(o => o.GroupId == message.GroupId && o.UserId == userId);
            if (!isMember)
                throw new Exception("Bạn không có quyền xoá tin nhắn này!");

            message.IsDeleted = true;
            message.DeletedBy = userId;
            message.DeletedDate = DateTime.UtcNow;

            this.context.Messages.AddOrUpdate(message);
            this.context.SaveChanges();

            this.EmitEventToClient();
        }

        public void DeleteAllUserMessage(Guid userId, Guid groupCode)
        {
            var messages = this.context.Messages.Where(o => o.GroupId == groupCode && o.CreatedBy == userId);
            var test = messages.ToList();
            this.context.Messages.RemoveRange(messages);
            this.context.SaveChanges();

            this.EmitEventToClient();
        }

        public void PinMessage(Guid userId, Guid messageId)
        {
            var message = this.context.Messages.FirstOrDefault(o => o.Id == messageId);
            if (message == null || message.IsDeleted == true)
                return;

            //  Only member in group can delete message
            var isMember = this.context.GroupUsers
                .Any(o => o.GroupId == message.GroupId && o.UserId == userId);
            if (!isMember)
                throw new Exception("Bạn không có quyền ghim tin nhắn này!");

            message.IsPinned = true;
            message.PinnedBy = userId;
            message.PinnedDate = DateTime.UtcNow;

            this.context.Messages.AddOrUpdate(message);
            this.context.SaveChanges();

            this.EmitEventToClient();
        }

        private void EmitEventToClient()
        {
            try
            {
                // Có thể tối ưu bằng cách chỉ gửi cho user trong nhóm chat
                this.chatHub.Clients.All.SendMessage("messageHubListener", true);
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
            }
        }
    }
}