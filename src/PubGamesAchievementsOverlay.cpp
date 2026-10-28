#include "PubGamesAchievementsOverlay.h"

#include <algorithm>
#include <cstdio>

#include <imgui.h>

#include <rex/input/input.h>
#include <rex/input/input_system.h>
#include <rex/runtime.h>
#include <rex/system/xtypes.h>
#include <rex/ui/immediate_drawer.h>
#include <rex/ui/keybinds.h>
#include <rex/ui/windowed_app_context.h>

namespace {
constexpr ImVec4 kParchment{0.94f, 0.84f, 0.63f, 1.0f};
constexpr ImVec4 kMutedParchment{0.70f, 0.61f, 0.46f, 1.0f};
constexpr ImVec4 kBrass{0.86f, 0.59f, 0.20f, 1.0f};
constexpr ImVec4 kLocked{0.48f, 0.43f, 0.36f, 1.0f};
constexpr ImVec4 kUnlockedBand{0.36f, 0.20f, 0.08f, 0.72f};
}

PubGamesAchievementsOverlay::PubGamesAchievementsOverlay(
    rex::ui::ImGuiDrawer* imgui_drawer, rex::ui::ImmediateDrawer* immediate_drawer,
    rex::Runtime* runtime, rex::system::AchievementManager* achievements,
    ImFont* pub_games_font)
    : ImGuiDialog(imgui_drawer),
      runtime_(runtime),
      achievements_(achievements),
      input_system_(runtime && runtime->input_system()
                        ? static_cast<rex::input::InputSystem*>(runtime->input_system())
                        : nullptr),
      icon_cache_(immediate_drawer, runtime),
      pub_games_font_(pub_games_font) {
  if (input_system_) {
    input_system_->AddUIInputBlocker();
  }
}

PubGamesAchievementsOverlay::~PubGamesAchievementsOverlay() {
  if (input_system_) {
    input_system_->RemoveUIInputBlocker();
  }
}

rex::ui::ImmediateTexture* PubGamesAchievementsOverlay::GetIcon(
    const rex::system::AchievementInfo& achievement) {
  return icon_cache_.GetIcon(achievement);
}

void PubGamesAchievementsOverlay::PollController() {
  if (!input_system_) {
    return;
  }
  rex::input::X_INPUT_STATE state{};
  if (input_system_->GetStateForUI(0, &state) != 0) {
    return;
  }
  const uint16_t buttons = state.gamepad.buttons;
  const uint16_t pressed = buttons & ~previous_buttons_;
  previous_buttons_ = buttons;
  if (pressed & rex::input::X_INPUT_GAMEPAD_B) {
    RequestClose();
  }
  if (pressed & rex::input::X_INPUT_GAMEPAD_LEFT_SHOULDER) {
    requested_scroll_ = -110.0f;
  }
  if (pressed & rex::input::X_INPUT_GAMEPAD_RIGHT_SHOULDER) {
    requested_scroll_ = 110.0f;
  }
}

void PubGamesAchievementsOverlay::RequestClose() {
  if (close_requested_ || !runtime_ || !runtime_->app_context()) {
    return;
  }
  close_requested_ = true;
  runtime_->app_context()->CallInUIThreadDeferred(
      []() { rex::ui::InvokeBind("bind_achievements"); });
}

void PubGamesAchievementsOverlay::OnDraw(ImGuiIO& io) {
  PollController();
  if (ImGui::IsKeyPressed(ImGuiKey_Escape)) {
    RequestClose();
  }

  ImGui::GetBackgroundDrawList()->AddRectFilled(
      ImVec2(0.0f, 0.0f), io.DisplaySize, IM_COL32(10, 5, 2, 185));

  const float width = std::min(820.0f, io.DisplaySize.x - 80.0f);
  const float height = std::min(680.0f, io.DisplaySize.y - 70.0f);
  ImGui::SetNextWindowPos(ImVec2(io.DisplaySize.x * 0.5f, io.DisplaySize.y * 0.5f),
                          ImGuiCond_Always, ImVec2(0.5f, 0.5f));
  ImGui::SetNextWindowSize(ImVec2(width, height), ImGuiCond_Always);
  ImGui::SetNextWindowBgAlpha(0.98f);

  ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(28.0f, 22.0f));
  ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 8.0f);
  ImGui::PushStyleVar(ImGuiStyleVar_WindowBorderSize, 2.0f);
  ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing, ImVec2(9.0f, 7.0f));
  ImGui::PushStyleColor(ImGuiCol_WindowBg, ImVec4(0.12f, 0.065f, 0.035f, 1.0f));
  ImGui::PushStyleColor(ImGuiCol_Border, kBrass);
  ImGui::PushStyleColor(ImGuiCol_Separator, ImVec4(0.48f, 0.29f, 0.11f, 0.82f));
  ImGui::PushStyleColor(ImGuiCol_ChildBg, ImVec4(0.065f, 0.035f, 0.02f, 0.82f));
  ImGui::PushStyleColor(ImGuiCol_ScrollbarBg, ImVec4(0.07f, 0.04f, 0.02f, 1.0f));
  ImGui::PushStyleColor(ImGuiCol_ScrollbarGrab, ImVec4(0.58f, 0.36f, 0.13f, 1.0f));
  if (pub_games_font_) {
    ImGui::PushFont(pub_games_font_);
  }

  constexpr ImGuiWindowFlags flags = ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize |
                                     ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse |
                                     ImGuiWindowFlags_NoSavedSettings;
  if (ImGui::Begin("Achievements##pub_games", nullptr, flags)) {
    const auto achievement_list = achievements_->ListAchievements();
    int unlocked_count = 0;
    int total_gs = 0;
    int earned_gs = 0;
    for (const auto& achievement : achievement_list) {
      total_gs += static_cast<int>(achievement.gamerscore);
      if (achievements_->IsUnlocked(achievement.id)) {
        ++unlocked_count;
        earned_gs += static_cast<int>(achievement.gamerscore);
      }
    }

    ImGui::SetWindowFontScale(1.45f);
    ImGui::TextColored(kBrass, "ACHIEVEMENTS");
    ImGui::SetWindowFontScale(1.0f);
    ImGui::TextColored(kParchment, "%d of %d unlocked", unlocked_count,
                       static_cast<int>(achievement_list.size()));
    ImGui::SameLine();
    ImGui::TextColored(kBrass, "  %dG / %dG", earned_gs, total_gs);
    const float fraction = achievement_list.empty()
                               ? 0.0f
                               : static_cast<float>(unlocked_count) /
                                     static_cast<float>(achievement_list.size());
    char progress_text[64];
    std::snprintf(progress_text, sizeof(progress_text), "%d%% complete",
                  static_cast<int>(fraction * 100.0f));
    ImGui::TextColored(kMutedParchment, "%s", progress_text);
    ImGui::PushStyleColor(ImGuiCol_PlotHistogram, kBrass);
    ImGui::ProgressBar(fraction, ImVec2(-1.0f, 8.0f), "");
    ImGui::PopStyleColor();
    ImGui::Spacing();
    ImGui::Separator();
    ImGui::Spacing();

    ImGui::BeginChild("##achievement_list", ImVec2(0.0f, -42.0f), false);
    if (requested_scroll_ != 0.0f) {
      ImGui::SetScrollY(std::max(0.0f, ImGui::GetScrollY() + requested_scroll_));
      requested_scroll_ = 0.0f;
    }
    ImDrawList* draw_list = ImGui::GetWindowDrawList();
    for (const auto& achievement : achievement_list) {
      const bool unlocked = achievements_->IsUnlocked(achievement.id);
      const std::string& description =
          unlocked ? achievement.description : achievement.unachieved_description;
      ImGui::PushID(static_cast<int>(achievement.id));
      const ImVec2 row_start = ImGui::GetCursorScreenPos();
      draw_list->ChannelsSplit(2);
      draw_list->ChannelsSetCurrent(1);
      ImGui::Dummy(ImVec2(0.0f, 5.0f));
      if (rex::ui::ImmediateTexture* icon = GetIcon(achievement)) {
        const ImVec4 tint = unlocked ? ImVec4(1, 1, 1, 1) : ImVec4(0.38f, 0.35f, 0.31f, 0.8f);
        ImGui::ImageWithBg(reinterpret_cast<ImTextureID>(icon), ImVec2(52.0f, 52.0f),
                           ImVec2(0, 0), ImVec2(1, 1), ImVec4(0, 0, 0, 0), tint);
      } else {
        ImGui::Dummy(ImVec2(52.0f, 52.0f));
      }
      ImGui::SameLine();
      ImGui::BeginGroup();
      ImGui::TextColored(unlocked ? kParchment : kLocked, "%s", achievement.label.c_str());
      ImGui::SameLine();
      ImGui::TextColored(kBrass, "  %dG", static_cast<int>(achievement.gamerscore));
      ImGui::PushStyleColor(ImGuiCol_Text, unlocked ? kMutedParchment : kLocked);
      ImGui::TextWrapped("%s", description.c_str());
      ImGui::PopStyleColor();
      ImGui::EndGroup();
      ImGui::Dummy(ImVec2(0.0f, 5.0f));
      const ImVec2 row_end = ImGui::GetCursorScreenPos();
      if (unlocked) {
        draw_list->ChannelsSetCurrent(0);
        draw_list->AddRectFilled(ImVec2(row_start.x, row_start.y),
                                 ImVec2(ImGui::GetWindowPos().x + ImGui::GetWindowWidth() - 3.0f,
                                        row_end.y),
                                 ImGui::GetColorU32(kUnlockedBand), 4.0f);
      }
      draw_list->ChannelsMerge();
      ImGui::Separator();
      ImGui::PopID();
    }
    ImGui::EndChild();
    ImGui::Separator();
    ImGui::TextColored(kBrass, "B");
    ImGui::SameLine();
    ImGui::TextColored(kParchment, "Close");
    ImGui::SameLine();
    ImGui::TextColored(kMutedParchment, "   Shoulder buttons to scroll");
  }
  ImGui::End();

  if (pub_games_font_) {
    ImGui::PopFont();
  }
  ImGui::PopStyleColor(6);
  ImGui::PopStyleVar(4);
}
