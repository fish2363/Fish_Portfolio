public enum EUILayer
{
    Default,    // 0: HUD, 항상 떠있는 고정 UI (스택 관리 X)
    Popup,      // 1: 인벤토리, 카드선택 등 팝업 (스택 관리 O)
    Fixed,      // 2: 전체화면을 덮는 메뉴 (기존 Override 대신 Fixed로 통일)
    Global      // 3: 최상단 경고창, 로딩창 등
}