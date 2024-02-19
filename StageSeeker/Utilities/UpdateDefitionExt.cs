using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace StageSeeker.Utilities;

public static class UpdateDefitionExt {
    public static UpdateDefinition<T> SetIfNotNull<T, TValue>(
        this UpdateDefinitionBuilder<T> builder,
        Expression<Func<T, TValue>> field,
        TValue value) {
            if(value != null)
            return builder.Set(field, value);
            else
                return Builders<T>.Update.Combine();
        }
}