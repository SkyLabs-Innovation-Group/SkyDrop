using System;
using System.Collections.Generic;
using System.Text;
using MvvmCross.ViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
    public abstract class BaseViewModel<TParameter> : BaseViewModel, IMvxViewModel<TParameter>
        where TParameter : notnull
    {
        protected BaseViewModel(ISingletonService singletonService) : base(singletonService)
        {
        }

        public abstract void Prepare(TParameter parameter);
    }
}
