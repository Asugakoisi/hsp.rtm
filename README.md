# hsp.rtm
HSPをリアルタイムデバッグするためのプログラムです

## Usage
1. hsp.watcher.exeを起動すると, hsp.rtm.exeとnotepad.exeが起動します
2. notepad.exeにHSPを書くと, その結果をhsp.rtm.exeがリアルタイムデバッグします
3. 終了する際はhsp.rtm.exeを終了して下さい

## Caution
- バグが非常に多いです  
 - システムに対してクリティカルなバグはないと思っていますが, まだ開発段階であるということを理解した上でご利用下さい
- 実装出来ている命令・コマンドは限りがあります
 - 実装出来ているものについては[hsp.cs](https://github.com/kkrnt/hsp.cs)をご覧下さい

## Bugs
現在認識しているバグは以下です
- gosubの実装ミス
 - 多段的なgosubが処理出来ていません
   - 次Releaseで修正する予定です
- 複数のウィンドウは表示
 - 複数のウィンドウを表示することに対応する予定は今のところありません

## etc
何かありましたら[@kkrnt](https://twitter.com/kkrnt)まで連絡頂けると幸いです