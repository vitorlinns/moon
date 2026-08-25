using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moon.Api.Contracts.Addresses;
using Moon.Api.Contracts.Auth;
using Moon.Api.Data;
using Moon.Api.Domain;
using Moon.Api.Security;

namespace Moon.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/addresses")]
public class AddressesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var addresses = await dbContext.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(addresses.Select(ToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddressRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var validationError = Validate(request);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var isFirstAddress = !await dbContext.Addresses.AnyAsync(a => a.UserId == userId, cancellationToken);

        var address = new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Label = request.Label.Trim(),
            Recipient = request.Recipient.Trim(),
            Cep = NormalizeCep(request.Cep),
            Street = request.Street.Trim(),
            Number = request.Number.Trim(),
            Complement = string.IsNullOrWhiteSpace(request.Complement) ? null : request.Complement.Trim(),
            Neighborhood = request.Neighborhood.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim().ToUpperInvariant(),
            // primeiro endereço do usuário sempre nasce como padrão, senão a lista fica sem nenhum
            IsDefault = isFirstAddress || request.IsDefault,
        };

        if (address.IsDefault)
        {
            await UnsetOtherDefaultsAsync(userId, address.Id, cancellationToken);
        }

        dbContext.Addresses.Add(address);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(address));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AddressRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        // filtra por UserId (não só Id) pra que um usuário nunca consiga editar o endereço de outro
        var address = await dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (address is null)
        {
            return NotFound();
        }

        var validationError = Validate(request);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        address.Label = request.Label.Trim();
        address.Recipient = request.Recipient.Trim();
        address.Cep = NormalizeCep(request.Cep);
        address.Street = request.Street.Trim();
        address.Number = request.Number.Trim();
        address.Complement = string.IsNullOrWhiteSpace(request.Complement) ? null : request.Complement.Trim();
        address.Neighborhood = request.Neighborhood.Trim();
        address.City = request.City.Trim();
        address.State = request.State.Trim().ToUpperInvariant();

        if (request.IsDefault && !address.IsDefault)
        {
            await UnsetOtherDefaultsAsync(userId, address.Id, cancellationToken);
            address.IsDefault = true;
        }
        else if (!request.IsDefault)
        {
            address.IsDefault = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(address));
    }

    [HttpPost("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var address = await dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (address is null)
        {
            return NotFound();
        }

        await UnsetOtherDefaultsAsync(userId, address.Id, cancellationToken);
        address.IsDefault = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(address));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var address = await dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (address is null)
        {
            return NotFound();
        }

        var wasDefault = address.IsDefault;
        dbContext.Addresses.Remove(address);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (wasDefault)
        {
            // promove o endereço mais recente restante a padrão, pra nunca ficar sem nenhum
            var nextDefault = await dbContext.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
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
        await dbContext.Addresses
            .Where(a => a.UserId == userId && a.Id != excludingId && a.IsDefault)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.IsDefault, false), cancellationToken);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private static string NormalizeCep(string cep) => new(cep.Where(char.IsDigit).ToArray());

    private static string? Validate(AddressRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Label)) return "Informe um nome para o endereço.";
        if (string.IsNullOrWhiteSpace(request.Recipient)) return "Informe o nome de quem recebe.";
        if (!CepValidator.IsValid(request.Cep)) return "Informe um CEP válido.";
        if (string.IsNullOrWhiteSpace(request.Street)) return "Informe a rua.";
        if (string.IsNullOrWhiteSpace(request.Number)) return "Informe o número.";
        if (string.IsNullOrWhiteSpace(request.Neighborhood)) return "Informe o bairro.";
        if (string.IsNullOrWhiteSpace(request.City)) return "Informe a cidade.";
        if (!BrazilianStates.IsValid(request.State)) return "Informe um estado (UF) válido.";
        return null;
    }

    private static AddressResponse ToResponse(Address address) => new(
        address.Id, address.Label, address.Recipient, address.Cep, address.Street, address.Number,
        address.Complement, address.Neighborhood, address.City, address.State, address.IsDefault);
}
