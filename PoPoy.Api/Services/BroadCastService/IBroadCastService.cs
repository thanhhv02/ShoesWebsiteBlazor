﻿using PoPoy.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoPoy.Api.Services.BroadCastService
{
    public interface IBroadCastService
    {

        Task BroadCastNotifyUser(ChatDto broadCast);

        Task SendNotifyAll(NotificationDto notification);
        Task SendNotifyAllAdmin(NotificationDto notification);
        Task SendNotifyUserId(NotificationDto notification);
        Task BroadCastMessageUser(string userId);

        Task ReadNoti(Guid id);
        Task ReadAllNoti(Guid userId);
    }
}
