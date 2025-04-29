using System;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Controls;

namespace TextEditorUI
{
    public partial class MainWindow : Window
    {
        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateTextEditor();

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyTextEditor(IntPtr editor);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void SetText(IntPtr editor, string text);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetText(IntPtr editor);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Undo(IntPtr editor);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Redo(IntPtr editor);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CanUndo(IntPtr editor);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CanRedo(IntPtr editor);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void Copy(IntPtr editor, string text);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void Cut(IntPtr editor, ref IntPtr text);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void Paste(IntPtr editor, string text);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool SaveToFile(IntPtr editor, string path);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool LoadFromFile(IntPtr editor, string path);

        [DllImport("TextEditorCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeString(IntPtr str);

        private IntPtr _editorHandle;
        private bool _isUpdatingText = false;

        public MainWindow()
        {
            InitializeComponent();
            _editorHandle = CreateTextEditor();
            SetupCommands();
        }

        private void SetupCommands()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, Undo_Executed, Undo_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, Redo_Executed, Redo_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, Cut_Executed, Cut_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, Copy_Executed, Copy_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_Executed, Paste_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Save_Executed));
        }

        private void UpdateTextFromEditor()
        {
            IntPtr textPtr = GetText(_editorHandle);
            string text = Marshal.PtrToStringUni(textPtr);
            FreeString(textPtr);

            if (EditorTextBox.Text != text)
            {
                _isUpdatingText = true;
                EditorTextBox.Text = text;
                _isUpdatingText = false;
            }
        }

        private void EditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdatingText)
            {
                SetText(_editorHandle, EditorTextBox.Text);
            }
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Undo(_editorHandle);
            UpdateTextFromEditor();
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanUndo(_editorHandle);
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Redo(_editorHandle);
            UpdateTextFromEditor();
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanRedo(_editorHandle);
        }

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (EditorTextBox.SelectionLength > 0)
            {
                IntPtr textPtr = Marshal.StringToHGlobalUni(EditorTextBox.SelectedText);
                Cut(_editorHandle, ref textPtr);
                string newText = Marshal.PtrToStringUni(textPtr);
                Marshal.FreeHGlobal(textPtr);

                int selectionStart = EditorTextBox.SelectionStart;
                EditorTextBox.Text = EditorTextBox.Text.Remove(selectionStart, EditorTextBox.SelectionLength);
                EditorTextBox.SelectionStart = selectionStart;
                Clipboard.SetText(newText);
            }
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = EditorTextBox.SelectionLength > 0;
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (EditorTextBox.SelectionLength > 0)
            {
                Copy(_editorHandle, EditorTextBox.SelectedText);
                Clipboard.SetText(EditorTextBox.SelectedText);
            }
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = EditorTextBox.SelectionLength > 0;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string textToPaste = Clipboard.GetText();
                Paste(_editorHandle, textToPaste);

                int selectionStart = EditorTextBox.SelectionStart;
                EditorTextBox.Text = EditorTextBox.Text.Insert(selectionStart, textToPaste);
                EditorTextBox.SelectionStart = selectionStart + textToPaste.Length;
            }
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                if (LoadFromFile(_editorHandle, openFileDialog.FileName))
                {
                    UpdateTextFromEditor();
                }
                else
                {
                    MessageBox.Show("Failed to open file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                if (!SaveToFile(_editorHandle, saveFileDialog.FileName))
                {
                    MessageBox.Show("Failed to save file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontComboBox.SelectedItem is ComboBoxItem item && item.Tag is string fontFamily)
            {
                EditorTextBox.FontFamily = new System.Windows.Media.FontFamily(fontFamily);
            }
        }

        private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SizeComboBox.SelectedItem is ComboBoxItem item && double.TryParse(item.Content.ToString(), out double size))
            {
                EditorTextBox.FontSize = size;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            DestroyTextEditor(_editorHandle);
            base.OnClosed(e);
        }
    }
}