using System.Security.Claims;
using Library.Core.DTOs.Loans;
using Library.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

[ApiController]
[Route("api/loans")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    private int CurrentMemberId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] LoanCreateRequest request, CancellationToken cancellationToken)
    {
        var loan = await _loanService.CreateAsync(CurrentMemberId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var loan = await _loanService.GetByIdAsync(id, cancellationToken);
        return loan is null ? NotFound() : Ok(loan);
    }

    [HttpGet("my-loans")]
    public async Task<IActionResult> GetMyLoans(CancellationToken cancellationToken)
    {
        var loans = await _loanService.GetMyLoansAsync(CurrentMemberId, cancellationToken);
        return Ok(loans);
    }

    [HttpPut("{id:int}/return")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Return(int id, CancellationToken cancellationToken)
    {
        var loan = await _loanService.ReturnAsync(id, CurrentMemberId, cancellationToken);
        return Ok(loan);
    }
}
