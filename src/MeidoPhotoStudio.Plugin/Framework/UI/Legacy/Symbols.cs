namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public static class Symbols
{
    private const string LeftTriangleBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAATklEQVQ4y2NgGFLg////9v///39B jkaB////H/gPBaRqbv7///+v/0iAJOf+xwJIci5JBmBzLiEDmGgRVeR7gSqBSLVopFpCokpSpikAAL6D9WxzYwkYAAAAAElFTkSuQmCC";
    private const string RightTriangleBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAUklEQVQ4y83SsQ2AMBBD0YiKObIPO7ET87AG7aNKhRDRHSj8ASx926X8CmyomQA4sGKOBjR2LJmARr+We/q0PHPRmt6e8ROFcInhGVNHyl15CCdv9vHnv4NdawAAAABJRU5ErkJggg==";
    private const string UpTriangleBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAXUlEQVQ4y93TsQ2AIBRFUUpdg3lkJpwJ52ENKK+NiUZ9hE/JSSj/K26Cc3MDAhBGj1cgX28ZGdi5ReuxB+pjoADeMnDwlSzhlK03nNIO+gqnxN5wyn9QEU5JE/2XE60DGPfYQY7VAAAAAElFTkSuQmCC";
    private const string DownTriangleBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAaUlEQVQ4y92RsQ2AMAwEbYkC1sg8sAsbkJnCPFmDpDsaKkSMEV2+smT5ZJ9F+gmw4096AgSgOIYPILS2iA7AZp0xAdkYzsD45mIxAPMfoenLR+5C2+IMSHSJcwg1xQ2thqoWYL3qKv3mBOnbF8kKK4jEAAAAAElFTkSuQmCC";
    private const string LeftChevronBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAb0lEQVQ4y9XSsQ2DMBQEUJSkRMoODOJhUjIJK2QMuvRUVDRs4QmiFI8illJiLFPk+lf8u980fxF0WDCV4IDom/kofuCd8Av3XHjF4Jcnbrm4xZjgB/3RstaEI8KeuZwxWfkJVUqsMmO1R6ryyqdnA2NDt6LtJOPuAAAAAElFTkSuQmCC";
    private const string RightChevronBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAbklEQVQ4y83SvQ2CUBhGYYKUJO7gIHcYSiZxBcawo7eisnELJiAUjwU3IaH8LhrPACd5f6rq78ATL9yigsnGjBQRXDFmyYIuImkw2LnjEhH1WLPkgTYiSbkPeB/Lrb89aSxCUYnFM55xpLIr/5wPn223og3bTEUAAAAASUVORK5CYII=";
    private const string UpChevronBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAoklEQVQ4y93TLW4CARDFcQJZHOBqMISeAQeCY+CqUBwDu55jYNZWtJJgEKCQGMC3IPhhJgGxfDtGTea9/0smmSkU3rvQxOezcAUzLFB7FC4hc6oxio8EpAFusI1+eC/8FcAeXXSwwwG9W3Ab/xHQP5sPYvaH1iW4gXUY0xx9FNoK9byAaRgylHL0Mn7C850X8IsJqldW/MAS81eOK0HyRv9yBNn9z6eSmES1AAAAAElFTkSuQmCC";
    private const string DownChevronBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAApElEQVQ4y+XTr5IBUBTHccPQrKYohmfYtoLH0KRNHkPVPYaiCsQdRSCJCrol+AiOGeH6G3d/6c75/u6ZM79zbybz/4Q88ikwwQ8+7lwuY4l5Ck6dNUQuwQsYh2eUalDFOgy9BO8HW6Fya8Qv/Ibx+6reidoOn49Caof5gCYa2OOI1rNJ96LJBts4d19ZVS7CvGiA7Kv7LmKGBUrvPpoa6n/8b5wARRfPp0QdIAQAAAAASUVORK5CYII=";
    private const string PlusBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAJ0lEQVQ4y2NgGN7gPxTgU8NEqSUDbwAjup+J0sTIyEg1F4zGwrAAAIBSFAD7S0jUAAAAAElFTkSuQmCC";
    private const string MinusBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAI0lEQVQ4y2NgGAXDADAic/7///+fKE2MjHB9TKNhOAoYGBgA9EEEBGpCn/cAAAAASUVORK5CYII=";

    private static GUIContent leftTriangle;
    private static GUIContent rightTriangle;
    private static GUIContent upTriangle;
    private static GUIContent downTriangle;
    private static GUIContent leftChevron;
    private static GUIContent rightChevron;
    private static GUIContent upChevron;
    private static GUIContent downChevron;
    private static GUIContent plus;
    private static GUIContent minus;

    public static GUIContent LeftTriangle =>
        leftTriangle ??= new(UIUtility.LoadTextureFromBase64(16, 16, LeftTriangleBase64));

    public static GUIContent RightTriangle =>
        rightTriangle ??= new(UIUtility.LoadTextureFromBase64(16, 16, RightTriangleBase64));

    public static GUIContent UpTriangle =>
        upTriangle ??= new(UIUtility.LoadTextureFromBase64(16, 16, UpTriangleBase64));

    public static GUIContent DownTriangle =>
        downTriangle ??= new(UIUtility.LoadTextureFromBase64(16, 16, DownTriangleBase64));

    public static GUIContent LeftChevron =>
        leftChevron ??= new(UIUtility.LoadTextureFromBase64(16, 16, LeftChevronBase64));

    public static GUIContent RightChevron =>
        rightChevron ??= new(UIUtility.LoadTextureFromBase64(16, 16, RightChevronBase64));

    public static GUIContent UpChevron =>
        upChevron ??= new(UIUtility.LoadTextureFromBase64(16, 16, UpChevronBase64));

    public static GUIContent DownChevron =>
        downChevron ??= new(UIUtility.LoadTextureFromBase64(16, 16, DownChevronBase64));

    public static GUIContent Plus =>
        plus ??= new(UIUtility.LoadTextureFromBase64(16, 16, PlusBase64));

    public static GUIContent Minus =>
        minus ??= new(UIUtility.LoadTextureFromBase64(16, 16, MinusBase64));
}
