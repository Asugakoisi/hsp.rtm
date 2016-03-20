# hsp.rtm
HSPをリアルタイムデバッグするためのプログラムです

## Usage
1. hsp.watcher.exeを起動すると, hsp.rtm.exeとnotepad.exeが起動します
2. notepad.exeにHSPを書くと, その結果をhsp.rtm.exeがリアルタイムデバッグします
3. 終了する際はhsp.rtm.exeを終了して下さい

## Bugs
現在認識しているバグは以下です
- gosubの実装ミス
 - 多段的なgosubが処理出来ていません
   - 次Releaseで修正する予定です
- 複数のウィンドウは表示
 - 複数のウィンドウを表示することに対応する予定は今のところありません

## etc
- バグが非常に多いです  
 - システムに対してクリティカルなバグはないと思っていますが, まだ開発段階であるということを理解した上でご利用下さい
- 実装出来ている命令・コマンドは限りがあります

何かありましたら[@kkrnt](https://twitter.com/kkrnt)まで連絡頂けると幸いです

## LICENSE
[The MIT License](https://github.com/kkrnt/hsp.cs/blob/master/LICENSE)

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