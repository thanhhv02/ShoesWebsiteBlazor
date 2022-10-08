﻿using Microsoft.AspNetCore.SignalR.Client;
using PoPoy.Shared.Dto;
using PoPoy.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoPoy.Admin.Services.BroadCastService
{
    public interface IBroadCastService
    {

        Task<ServiceResponse<List<NotificationDto>>> GetNotificationsByUserJwt();
        Task ReadNoti(Guid notiId);
        Task ReadAllNoti();
        Task SendNotiAllAdmin(NotiSendConfig dto);

        Task SendNotiUserId(NotiSendConfig dto, Guid UserId);


        Task<HubConnection> BuidHubWithToken();

        Task StartAsync(HubConnection hubConnection);
    }
}
