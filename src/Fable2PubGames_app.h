// Fable2PubGames - ReXGlue Recompiled Project
//
// Customize your app by overriding virtual hooks from rex::ReXApp.

#pragma once

#include <filesystem>

#include <rex/rex_app.h>

#include "PubGamesAchievementsOverlay.h"

class Fable2pubgamesApp : public rex::ReXApp {
 public:
  using rex::ReXApp::ReXApp;

  static std::unique_ptr<rex::ui::WindowedApp> Create(
      rex::ui::WindowedAppContext& ctx) {
    return std::unique_ptr<Fable2pubgamesApp>(new Fable2pubgamesApp(ctx, "Fable2PubGames",
        PPCImageConfig));
  }

  // Override virtual hooks for customization:
  // void OnPostInitLogging() override {}
  void OnPreSetup(rex::RuntimeConfig& config) override {
    if (config.gpu_plugin.empty()) {
      config.gpu_plugin = "xenos";
    }
  }
  void OnConfigureFonts(ImFontAtlas* atlas) override {
    const auto font_path = game_data_root() / "media" / "font_en.ttf";
    if (std::filesystem::is_regular_file(font_path)) {
      pub_games_font_ = atlas->AddFontFromFileTTF(font_path.string().c_str(), 20.0f);
    }
  }
  void OnConfigureStyle(ImGuiStyle& imgui_style, rex::ui::Style& ui_style) override {
    imgui_style.Colors[ImGuiCol_WindowBg] = ImVec4(0.12f, 0.065f, 0.035f, 0.98f);
    imgui_style.Colors[ImGuiCol_Text] = ImVec4(0.94f, 0.84f, 0.63f, 1.0f);
    imgui_style.Colors[ImGuiCol_Border] = ImVec4(0.86f, 0.59f, 0.20f, 1.0f);
    imgui_style.WindowRounding = 8.0f;
    imgui_style.FrameRounding = 5.0f;
    imgui_style.ScrollbarRounding = 5.0f;
    ui_style.toast.title = ImVec4(0.86f, 0.59f, 0.20f, 1.0f);
    ui_style.toast.text = ImVec4(0.94f, 0.84f, 0.63f, 1.0f);
  }
  std::unique_ptr<rex::ui::ImGuiDialog> CreateAchievementsOverlay() override {
    if (!runtime() || !imgui_drawer() || !immediate_drawer()) {
      return nullptr;
    }
    return std::make_unique<PubGamesAchievementsOverlay>(
        imgui_drawer(), immediate_drawer(), runtime(), &achievements(), pub_games_font_);
  }
  // void OnLoadXexImage(std::string& xex_image) override {}
  // void OnPostLoadXexImage() override {}
  // void OnPostSetup() override {}
  // void OnCreateDialogs(rex::ui::ImGuiDrawer* drawer) override {}
  // std::unique_ptr<rex::ui::ImGuiDialog> CreateAchievementsOverlay() override;
  // std::unique_ptr<rex::ui::AchievementNotificationDialog>
  // CreateAchievementNotificationDialog() override;
  // void OnShutdown() override {}
  void OnConfigurePaths(rex::PathConfig& paths) override {
    const auto exe_dir = rex::filesystem::GetExecutableFolder();

    if (paths.game_data_root.empty()) {
      if (std::filesystem::is_regular_file(exe_dir / "default.xex")) {
        paths.game_data_root = exe_dir;
      }
    }

    paths.user_data_root = exe_dir / "saves";
    paths.cache_root = exe_dir / "cache";

    std::error_code error;
    std::filesystem::create_directories(paths.user_data_root, error);
    std::filesystem::create_directories(paths.cache_root, error);
  }

 private:
  ImFont* pub_games_font_ = nullptr;
};
