namespace utils 
{
    public static class Extensions
    {
        public static T[] Append<T>(this T[] array, T item)
        {
            if (array == null) return new T[] { item };
            T[] result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[array.Length] = item;
            return result;
        }
    }
}