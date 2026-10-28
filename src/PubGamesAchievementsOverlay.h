#pragma once

#include <functional>

#include <rex/system/achievement_manager.h>
#include <rex/ui/imgui_dialog.h>
#include <rex/ui/overlay/achievement_icon_cache.h>

struct ImFont;

namespace rex {
class Runtime;
namespace input {
class InputSystem;
}
namespace ui {
class ImmediateDrawer;
class ImmediateTexture;
}
}

class PubGamesAchievementsOverlay final : public rex::ui::ImGuiDialog {
 public:
  PubGamesAchievementsOverlay(rex::ui::ImGuiDrawer* imgui_drawer,
                              rex::ui::ImmediateDrawer* immediate_drawer,
                              rex::Runtime* runtime,
                              rex::system::AchievementManager* achievements,
                              ImFont* pub_games_font);
  ~PubGamesAchievementsOverlay() override;

 protected:
  void OnDraw(ImGuiIO& io) override;

 private:
  rex::ui::ImmediateTexture* GetIcon(const rex::system::AchievementInfo& achievement);
  void PollController();
  void RequestClose();

  rex::Runtime* runtime_ = nullptr;
  rex::system::AchievementManager* achievements_ = nullptr;
  rex::input::InputSystem* input_system_ = nullptr;
  rex::ui::AchievementIconCache icon_cache_;
  ImFont* pub_games_font_ = nullptr;
  uint16_t previous_buttons_ = 0;
  float requested_scroll_ = 0.0f;
  bool close_requested_ = false;
};
