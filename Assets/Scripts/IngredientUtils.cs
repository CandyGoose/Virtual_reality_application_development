public static class IngredientUtils
{
    public static IngredientKind GetKindByName(string ingredientName)
    {
        string lower = ingredientName.ToLower();

        if (lower.Contains("cristal") || lower.Contains("crystal"))
            return IngredientKind.Crystal;

        if (lower.Contains("mushroom"))
            return IngredientKind.Mushroom;

        if (lower.Contains("flask"))
            return IngredientKind.Flask;   

        if (lower.Contains("jar"))
            return IngredientKind.Jar;     

        if (lower.Contains("bag"))
            return IngredientKind.Bag;     

        if (lower.Contains("minipot"))
            return IngredientKind.Minipot;

        return IngredientKind.None;
    }
}
