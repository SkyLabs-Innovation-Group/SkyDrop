using System;
using System.Collections.Generic;
using System.Text;
using MvvmCross.ViewModels;
using SkyDrop.Core.Services.Api;

namespace SkyDrop.Core.ViewModels
{
    public abstract class BaseViewModel<TParameter, TResult> : BaseViewModelResult<TResult>, IMvxViewModel<TParameter, TResult>
        where TParameter : notnull
        where TResult : notnull
    {
        protected BaseViewModel(ISingletonService singletonService) : base(singletonService)
        {
        }

        public abstract void Prepare(TParameter parameter);
    }
}
