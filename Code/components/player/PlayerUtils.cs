static class PlayerUtils
{
    public static Vector2 GetMousePosition()
    {
        var screenCenter = new Vector2( Screen.Size.x, Screen.Size.y ) * 0.5f;
        var mousePosition = new Vector2( Mouse.Position.x, Mouse.Position.y );

        return new Vector2( mousePosition - screenCenter - new Vector2( 0, PlayerController2D.EYE_POSITION_Z ) );
    }
}
