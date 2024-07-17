using System;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IValidationService
    {
        void Validate<T>(T entity);
    }
}
