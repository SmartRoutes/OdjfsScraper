﻿namespace OdjfsScraper.Parse
{
    public interface IChildCareParser<T>
    {
        T Parse(T childCare, byte[] bytes);
    }
}