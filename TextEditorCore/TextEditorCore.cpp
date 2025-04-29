#include "pch.h"
#include "TextEditorCore.h"
#include <string>
#include <vector>
#include <stack>
#include <fstream>
#include <codecvt>
#include <locale>
#include <memory>

class TextEditorImpl {
public:
    TextEditorImpl() {
        m_currentText = L"";
        m_undoStack.push(m_currentText);
    }

    void SetText(const std::wstring& text) {
        if (m_currentText != text) {
            m_redoStack = std::stack<std::wstring>(); // Очищаем redo при новом изменении
            m_currentText = text;
            m_undoStack.push(m_currentText);
        }
    }

    const std::wstring& GetText() const {
        return m_currentText;
    }

    void Undo() {
        if (CanUndo()) {
            m_redoStack.push(m_currentText);
            m_undoStack.pop();
            m_currentText = m_undoStack.top();
        }
    }

    void Redo() {
        if (CanRedo()) {
            m_undoStack.push(m_currentText);
            m_currentText = m_redoStack.top();
            m_redoStack.pop();
        }
    }

    bool CanUndo() const {
        return m_undoStack.size() > 1;
    }

    bool CanRedo() const {
        return !m_redoStack.empty();
    }

    void Copy(const std::wstring& text) {
        m_clipboard = text;
    }

    void Cut(std::wstring& text) {
        m_clipboard = text;
        text.clear();
    }

    void Paste(const std::wstring& text) {
        m_currentText += text;
        m_undoStack.push(m_currentText);
    }

    bool SaveToFile(const std::wstring& path) {
        std::wofstream file(path);
        if (file) {
            file.imbue(std::locale(std::locale::empty(), new std::codecvt_utf8<wchar_t>));
            file << m_currentText;
            return file.good();
        }
        return false;
    }

    bool LoadFromFile(const std::wstring& path) {
        std::wifstream file(path);
        if (file) {
            file.imbue(std::locale(std::locale::empty(), new std::codecvt_utf8<wchar_t>));
            std::wstring content((std::istreambuf_iterator<wchar_t>(file)), std::istreambuf_iterator<wchar_t>());
            SetText(content);
            return true;
        }
        return false;
    }

private:
    std::wstring m_currentText;
    std::stack<std::wstring> m_undoStack;
    std::stack<std::wstring> m_redoStack;
    std::wstring m_clipboard;
};

// Реализация C-интерфейса
extern "C" {
    TEXTEDITORCORE_API void* CreateTextEditor() {
        return new TextEditorImpl();
    }

    TEXTEDITORCORE_API void DestroyTextEditor(void* editor) {
        delete static_cast<TextEditorImpl*>(editor);
    }

    TEXTEDITORCORE_API void SetText(void* editor, const wchar_t* text) {
        static_cast<TextEditorImpl*>(editor)->SetText(text);
    }

    TEXTEDITORCORE_API const wchar_t* GetText(void* editor) {
        const std::wstring& text = static_cast<TextEditorImpl*>(editor)->GetText();
        wchar_t* result = new wchar_t[text.size() + 1];
        wcscpy_s(result, text.size() + 1, text.c_str());
        return result;
    }

    TEXTEDITORCORE_API void Undo(void* editor) {
        static_cast<TextEditorImpl*>(editor)->Undo();
    }

    TEXTEDITORCORE_API void Redo(void* editor) {
        static_cast<TextEditorImpl*>(editor)->Redo();
    }

    TEXTEDITORCORE_API bool CanUndo(void* editor) {
        return static_cast<TextEditorImpl*>(editor)->CanUndo();
    }

    TEXTEDITORCORE_API bool CanRedo(void* editor) {
        return static_cast<TextEditorImpl*>(editor)->CanRedo();
    }

    TEXTEDITORCORE_API void Copy(void* editor, const wchar_t* text) {
        static_cast<TextEditorImpl*>(editor)->Copy(text);
    }

    TEXTEDITORCORE_API void Cut(void* editor, wchar_t** text) {
        std::wstring str(*text);
        static_cast<TextEditorImpl*>(editor)->Cut(str);
        *text = new wchar_t[str.size() + 1];
        wcscpy_s(*text, str.size() + 1, str.c_str());
    }

    TEXTEDITORCORE_API void Paste(void* editor, const wchar_t* text) {
        static_cast<TextEditorImpl*>(editor)->Paste(text);
    }

    TEXTEDITORCORE_API bool SaveToFile(void* editor, const wchar_t* path) {
        return static_cast<TextEditorImpl*>(editor)->SaveToFile(path);
    }

    TEXTEDITORCORE_API bool LoadFromFile(void* editor, const wchar_t* path) {
        return static_cast<TextEditorImpl*>(editor)->LoadFromFile(path);
    }

    TEXTEDITORCORE_API void FreeString(wchar_t* str) {
        delete[] str;
    }
}