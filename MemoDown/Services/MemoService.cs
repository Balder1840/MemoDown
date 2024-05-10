using MemoDown.Models;
using MemoDown.Store;

namespace MemoDown.Services
{
    public class MemoService
    {
        private readonly MemoStore _store;
        private MemoItem? _selectedMemo;

        public MemoItem? SelectedMemo => _selectedMemo;
        public MemoItem? SelectedSidebarMemo => SelectedMemo?.Parent;
        public MemoService(MemoStore store)
        {
            _store = store;

            InitializeSelectedMemo(RootMemo);
        }

        public MemoItem RootMemo => _store.Memo;

        public void SetSelectedMemo(MemoItem? memo)
        {
            _selectedMemo = memo;
        }

        public MemoItem? GetSelectedMemoFromSidebar(MemoItem? sidebarMemo)
        {
            var seletedMemo = sidebarMemo?.Children?.FirstOrDefault(m => !m.IsDirectory);
            SetSelectedMemo(seletedMemo);
            return seletedMemo;
        }

        private void InitializeSelectedMemo(MemoItem memo)
        {
            if (memo == null || memo.Children == null)
            {
                return;
            }

            if (memo.Children.Any(m => !m.IsDirectory))
            {
                _selectedMemo = memo.Children.First(m => !m.IsDirectory);
                return;
            }

            if (memo.Children.Any(m => m.IsDirectory))
            {
                InitializeSelectedMemo(memo.Children.First(m => m.IsDirectory));
            }
        }
    }
}
