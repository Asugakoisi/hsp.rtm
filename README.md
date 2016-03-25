# hsp.rtm
HSPをリアルタイムデバッグするためのプログラムです  
試験的にVisual Studio Codeの拡張機能として動かしています

## Usage
1. ```hsp.rtm.sln```を開き, プロジェクト全てをビルドする
2. 作成された```hsp.rtm.exe```と```hsp.watcher.exe```を```C:\\hsp.rtm```にコピーする
3. ```vscode-extension```フォルダをVisual Studio Codeで開く
4. F5等で実行する
5. 終了するときは, Visual Studio Codeのデバッグモードと```hsp.rtm.exe```を閉じて下さい

## Bugs
現在認識しているバグは以下です
- 洒落にならないレベルで重い
 - Visual Studio Codeと連動させる部分で恐らくバグがあると思います
- gosubの実装ミス
 - 多段的なgosubが処理出来ていません
   - 次Releaseで修正する予定です
- 複数のウィンドウは表示
 - 複数のウィンドウを表示することに対応する予定は今のところありません

## ETC
- Visual Studio Code上のシンタックスハイライトはpotato4dさんのtmLanguageファイルを利用させて頂きました
 - [https://github.com/potato4d/sublime-HSP](https://github.com/potato4d/sublime-HSP)
- バグが非常に多いです  
 - システムに対してクリティカルなバグはないと思っていますが, まだ開発段階であるということを理解した上でご利用下さい
- 実装出来ている命令・コマンドは限りがあります

何かありましたら[@kkrnt](https://twitter.com/kkrnt)まで連絡頂けると幸いです

## LICENSE
[The MIT License](https://github.com/kkrnt/hsp.cs/blob/master/LICENSE)
  
potato4dさんのsublime-HSPもMIT Licenseです  
[https://github.com/potato4d/sublime-HSP/blob/master/LICENSE](https://github.com/potato4d/sublime-HSP/blob/master/LICENSE)

## 実装済みのもの
### Basic Grammar
- if
- else
- for
- next
- while
- wend
- repeat
- loop
- switch
- swend
- swbreak
- case
- default
- _break
- _continue
- goto
- gosub

### Function
- int
- double
- str
- abs
- absf
- sin
- cos
- tan
- atan
- expf
- logf
- powf
- sqrt
- instr
- strlen
- limit
- limitf
- length
- length2
- length3
- length4
- gettime
- deg2rad
- rad2deg
- strmid
- strtrim
- rnd

### Command
- print
- mes
- exist
- delete
- mkdir
- split
- bcopy
- strrep
- dim
- ddim
- chdir
- ginfo_mx
- ginfo_my
- end
- stop
- screen
- title
- circle
- boxf
- pos
- line
- color
- wait
- objsize
- dialog

### Macro
- M_PI
- and
- not
- or
- xor
- mousex
- mousey
- dir_desktop
- dir_exe
- dir_mydoc
- dir_sys
- dir_win
- dir_cur
- ginfo_sizex
- ginfo_sizey
- ginfo_r
- ginfo_g
- ginfo_b
- ginfo_cx
- ginfo_cy
- ginfo_dispx
- ginfo_dispy
- ginfo_wx1
- ginfo_wx2
- ginfo_wy1
- ginfo_wy2
- ginfo_sel
- hwnd
- __date__
- __time__
