using Azure.Messaging.ServiceBus;
using IdentityService.Api.Abstractions;
using IdentityService.Api.Dtos.Requests;
using IdentityService.Api.Dtos.Results;
using System.Text.Json;

namespace IdentityService.Api.Services;

public class VerificationService(HttpClient httpClient) : IVerificationService
{
    public async Task SendEmailVerificationAsync(string email, string token, CancellationToken ct = default)
    {
        var emailVerificationRequest = new EmailVerificationRequest(email);

        var result = await httpClient.PostAsJsonAsync("api/Verification/email/request", emailVerificationRequest, ct);

    }



    public async Task<VerifyEmailVerificationCodeResult> VerifyEmailVerificationCodeAsync(VerifyEmailVerificationCodeRequest request, CancellationToken ct = default)
    {
        var result = await httpClient.PostAsJsonAsync("api/Verification/email/verify", request, ct);

        if (result.IsSuccessStatusCode)
            return VerifyEmailVerificationCodeResult.Success(request.Email);

        return VerifyEmailVerificationCodeResult.Failed("Invalid verification code");
    }





}


//public class VerificationService(HttpClient httpClient, ServiceBusClient serviceBusClient) : IVerificationService
//{
//    public async Task SendEmailVerificationAsync(string email, string token, CancellationToken ct = default)
//    {
//        var emailVerificationRequest = new EmailVerificationRequest(email);

//        var result = await httpClient.PostAsJsonAsync("api/verification/email/request", emailVerificationRequest, ct);

//    }



//    public async Task<VerifyEmailVerificationCodeResult> VerifyEmailVerificationCodeAsync(VerifyEmailVerificationCodeRequest request, CancellationToken ct = default)
//    {
//        var result = await httpClient.PostAsJsonAsync("api/verification/verify", request, ct);

//        if (result.IsSuccessStatusCode)
//            return VerifyEmailVerificationCodeResult.Success(request.Email);

//        return VerifyEmailVerificationCodeResult.Failed("Invalid verification code");
//    }



//    public async Task PublicEmailVerificationAsync(string email, CancellationToken ct = default)
//    {
//        var emailVerificationRequest = new EmailVerificationRequest(email);
//        var json = JsonSerializer.Serialize(emailVerificationRequest);

//        var sender = serviceBusClient.CreateSender("email-queue");

//        var message = new ServiceBusMessage(json)
//        {
//            ContentType = "application/json",
//            Subject = "EmailVerificationRequest"
//        };

//        message.ApplicationProperties["MessageType"] = "EmailVerificationRequest";
//        message.ApplicationProperties["Recipent"] = email;

//        await sender.SendMessageAsync(message, ct);

//    }

//}