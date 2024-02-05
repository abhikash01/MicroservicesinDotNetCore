﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder
{
    public  class UpdateOrderCommandValidator: AbstractValidator<UpdateOrderCommand>
    {

        public UpdateOrderCommandValidator()
        {
            RuleFor(p => p.UserName)
            .NotEmpty().WithMessage("{UserName} is required")
            .NotNull()
            .MinimumLength(50).WithMessage("{UserName} should not exceed 50 characters");

            RuleFor(p => p.EmailAddress)
                .NotEmpty().WithMessage("{EmailAddress} is required");

            RuleFor(p => p.TotalPrice)
                .NotEmpty().WithMessage("{TotalPrice} is required")
                .GreaterThan(0).WithMessage("{TotalPrice} should be greater than 0");


        }

        
    }
}