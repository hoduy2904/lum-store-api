using LumStoreAPI.Application.DTOs.DiscountRuleDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

/// <summary>Quantity-based discount rules management.</summary>
[Route("api/discount-rules")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class DiscountRuleController : ControllerBase
{
    private readonly IDiscountRuleService _discountRuleService;

    public DiscountRuleController(IDiscountRuleService discountRuleService)
        => _discountRuleService = discountRuleService;

    /// <summary>GET /api/discount-rules?productId=&amp;activeOnly=</summary>
    [HttpGet]
    public async Task<IActionResult> GetRules([FromQuery] int? productId, [FromQuery] bool activeOnly = false)
    {
        var rules = await _discountRuleService.GetRulesAsync(productId, activeOnly);
        return Ok(APIResponse<IEnumerable<DiscountRuleGetDTO>>.Success(rules));
    }

    /// <summary>GET /api/discount-rules/{ruleId}</summary>
    [HttpGet("{ruleId:int}")]
    public async Task<IActionResult> GetRule(int ruleId)
    {
        var rule = await _discountRuleService.GetRuleAsync(ruleId);
        if (rule == null) return NotFound(APIResponseBase.Failure("NOT_FOUND", ["Rule not found"]));
        return Ok(APIResponse<DiscountRuleGetDTO>.Success(rule));
    }

    /// <summary>POST /api/discount-rules</summary>
    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] DiscountRuleUpsertDTO dto)
    {
        var rule = await _discountRuleService.CreateRuleAsync(dto);
        return CreatedAtAction(nameof(GetRule), new { ruleId = rule.RuleId },
            APIResponse<DiscountRuleGetDTO>.Success(rule, ["Rule created"]));
    }

    /// <summary>PUT /api/discount-rules/{ruleId}</summary>
    [HttpPut("{ruleId:int}")]
    public async Task<IActionResult> UpdateRule(int ruleId, [FromBody] DiscountRuleUpsertDTO dto)
    {
        var rule = await _discountRuleService.UpdateRuleAsync(ruleId, dto);
        return Ok(APIResponse<DiscountRuleGetDTO>.Success(rule, ["Rule updated"]));
    }

    /// <summary>DELETE /api/discount-rules/{ruleId}</summary>
    [HttpDelete("{ruleId:int}")]
    public async Task<IActionResult> DeleteRule(int ruleId)
    {
        var deleted = await _discountRuleService.DeleteRuleAsync(ruleId);
        if (!deleted) return NotFound(APIResponseBase.Failure("NOT_FOUND", ["Rule not found"]));
        return Ok(APIResponseBase.Success(["Rule deleted"]));
    }

    /// <summary>GET /api/discount-rules/calculate?productId=&amp;variantId=&amp;unitPrice=&amp;quantity= — Preview discounted price.</summary>
    [HttpGet("calculate")]
    [AllowAnonymous]
    public async Task<IActionResult> Calculate(
        [FromQuery] int productId,
        [FromQuery] int? variantId,
        [FromQuery] decimal unitPrice,
        [FromQuery] int quantity)
    {
        var discounted = await _discountRuleService.CalculateDiscountedPriceAsync(productId, variantId, unitPrice, quantity);
        return Ok(APIResponse<object>.Success(new
        {
            UnitPrice = unitPrice,
            DiscountedPrice = discounted,
            Savings = unitPrice - discounted,
            Quantity = quantity
        }));
    }
}
