using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moon.Api.Contracts.Auth;
using Moon.Api.Contracts.PaymentMethods;
using Moon.Api.Data;
using Moon.Api.Domain;

namespace Moon.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/payment-methods")]
public class PaymentMethodsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var paymentMethods = await dbContext.PaymentMethods
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.IsDefault)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(paymentMethods.Select(ToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create(PaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var validationError = Validate(request);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var isFirstCard = !await dbContext.PaymentMethods.AnyAsync(p => p.UserId == userId, cancellationToken);

        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Brand = request.Brand.Trim(),
            LastFourDigits = request.LastFourDigits,
            HolderName = request.HolderName.Trim(),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            // primeiro cartão do usuário sempre nasce como padrão, senão a lista fica sem nenhum
            IsDefault = isFirstCard || request.IsDefault,
        };

        if (paymentMethod.IsDefault)
        {
            await UnsetOtherDefaultsAsync(userId, paymentMethod.Id, cancellationToken);
        }

        dbContext.PaymentMethods.Add(paymentMethod);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(paymentMethod));
    }

    [HttpPost("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var paymentMethod = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);

        if (paymentMethod is null)
        {
            return NotFound();
        }

        await UnsetOtherDefaultsAsync(userId, paymentMethod.Id, cancellationToken);
        paymentMethod.IsDefault = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(paymentMethod));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        // filtra por UserId (não só Id) pra que um usuário nunca consiga remover o cartão de outro
        var paymentMethod = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);

        if (paymentMethod is null)
        {
            return NotFound();
        }

        var wasDefault = paymentMethod.IsDefault;
        dbContext.PaymentMethods.Remove(paymentMethod);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (wasDefault)
        {
            // promove o cartão mais recente restante a padrão, pra nunca ficar sem nenhum
            var nextDefault = await dbContext.PaymentMethods
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextDefault is not null)
            {
                nextDefault.IsDefault = true;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return NoContent();
    }

    private async Task UnsetOtherDefaultsAsync(Guid userId, Guid excludingId, CancellationToken cancellationToken)
    {
        await dbContext.PaymentMethods
            .Where(p => p.UserId == userId && p.Id != excludingId && p.IsDefault)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.IsDefault, false), cancellationToken);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private static string? Validate(PaymentMethodRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Brand)) return "Bandeira do cartão inválida.";
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.LastFourDigits, "^[0-9]{4}$"))
        {
            return "Número de cartão inválido.";
        }
        if (string.IsNullOrWhiteSpace(request.HolderName)) return "Informe o nome impresso no cartão.";
        if (request.ExpiryMonth is < 1 or > 12) return "Validade do cartão inválida.";

        var now = DateTimeOffset.UtcNow;
        if (request.ExpiryYear < now.Year || request.ExpiryYear > now.Year + 30)
        {
            return "Validade do cartão inválida.";
        }

        // primeiro dia do mês seguinte ao de validade — cartão vale até o fim do mês de expiração
        var expiresAtEndOfMonth = new DateTimeOffset(request.ExpiryYear, 1, 1, 0, 0, 0, TimeSpan.Zero)
            .AddMonths(request.ExpiryMonth);

        if (expiresAtEndOfMonth <= now)
        {
            return "Cartão vencido.";
        }

        return null;
    }

    private static PaymentMethodResponse ToResponse(PaymentMethod paymentMethod) => new(
        paymentMethod.Id, paymentMethod.Brand, paymentMethod.LastFourDigits, paymentMethod.HolderName,
        paymentMethod.ExpiryMonth, paymentMethod.ExpiryYear, paymentMethod.IsDefault);
}
