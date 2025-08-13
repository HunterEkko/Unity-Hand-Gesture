# VR手勢辨識基本概念

預設節點圖例:

<img src="https://docs.unity3d.com/Packages/com.unity.xr.hands@1.5/manual/images/xrhands-data-model.png" alt="圖片" width="350">

| 英文 | 中文 |
| ---- | ---- |
| Tip | 指尖 |
| Distal | 遠端指骨 |
| Intermediate | 中節指骨 |
| Proximal | 近端指骨 |
| Metacarpal | 掌骨 |
| Palm | 手掌 |
| Wrist | 手腕 |

---
## 邏輯架構
- XRHandTrackingEvents(Component)，可以選擇要接收哪隻手的事件。 此元件會訂閱 XRHandSubsystem，當手部資料更新時會發送事件。
- XRHandSubsystem 每一幀 (Frame) 會更新兩次，第一次更新 -> 遊戲邏輯與互動；第二次更新 -> 渲染與視覺表現
- XR Hand Skeleton Driver(Component)，自動把手部資料套用到骨架模型的 Transform，只要骨架的關節名稱與官方的 XRHandJointID 對應名稱一致，就能用 Find Joints 自動綁定。關節的擺動會根據父節點來計算，所以必須保留正確的父子層級。

---
## 手勢
### Finger Shape
- Full Curl：整根手指的彎曲程度
- Base Curl：近端指骨的彎曲程度
- Tip Curl：指尖的彎曲程度
- Pinch：手指與拇指的距離
- Spread：相鄰手指之間的張開角度

### Hand Orientation
#### Hand Axis
- Fingers Extended Direction：手指延伸方向
- Thumb Extended Direction：大拇指延伸方向
- Palm Direction：手掌朝向

#### Alignment (比較手部軸向 與 參考方向)
- Aligns with：相同方向
- Perpendicular to：垂直
- Opposite To：反向平行 (相差180度)

#### Reference direction：
- Origin Up：XR Origin 的正 Y 軸方向，
  - 偵測「手掌朝上」。
- Hand to Head：面向玩家
  - 偵測「手掌面向自己的臉」。
- Nose Direction：耳朵方向
  - 偵測「手指指向自己正前方」。
- Chin Direction：下巴方向
  - 偵測「手掌朝向地面」。
- Ear Direction：從耳朵往外的方向（左手手勢對應左耳，右手手勢對應右耳)
  - 偵測「手掌朝向側邊」。
 
#### Ignore Y Component：
- 這會將手部軸向與參考方向投影到 X-Z 平面，也就是只比較「水平面上的朝向」。

#### User Conditions 與 Target Conditions 的差別
> User Conditions：參考方向是使用者本身的方向

> Target Conditions：參考方向是場景中某個目標 GameObject 的方向，如果沒有指定目標，則視為任何角度皆可成立。
