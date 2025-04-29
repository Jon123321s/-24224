#pragma once

#ifdef TEXTEDITORCORE_EXPORTS
#define TEXTEDITORCORE_API __declspec(dllexport)
#else
#define TEXTEDITORCORE_API __declspec(dllimport)
#endif

#include <string>
#include <vector>
#include <stack>
#include <memory>

// Forward declaration
class TextEditorImpl;

extern "C" {
    // �������������/��������������� ���������
    TEXTEDITORCORE_API void* CreateTextEditor();
    TEXTEDITORCORE_API void DestroyTextEditor(void* editor);

    // ������ � �������
    TEXTEDITORCORE_API void SetText(void* editor, const wchar_t* text);
    TEXTEDITORCORE_API const wchar_t* GetText(void* editor);

    // �������� ��������������
    TEXTEDITORCORE_API void Undo(void* editor);
    TEXTEDITORCORE_API void Redo(void* editor);
    TEXTEDITORCORE_API bool CanUndo(void* editor);
    TEXTEDITORCORE_API bool CanRedo(void* editor);

    // ����� ������
    TEXTEDITORCORE_API void Copy(void* editor, const wchar_t* text);
    TEXTEDITORCORE_API void Cut(void* editor, wchar_t** text);
    TEXTEDITORCORE_API void Paste(void* editor, const wchar_t* text);

    // ������ � �������
    TEXTEDITORCORE_API bool SaveToFile(void* editor, const wchar_t* path);
    TEXTEDITORCORE_API bool LoadFromFile(void* editor, const wchar_t* path);

    // ���������� ������� ��� �����
    TEXTEDITORCORE_API void FreeString(wchar_t* str);
}