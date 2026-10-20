// 音声ファイルを macOS の音声認識で文字にします（日本語）。ROM から取り出した声の中身を確かめる用。
//   Tools/bin/transcribe <wav ...>
import Foundation
import Speech

let files = CommandLine.arguments.dropFirst()
let sem = DispatchSemaphore(value: 0)
SFSpeechRecognizer.requestAuthorization { _ in sem.signal() }
sem.wait()
guard let rec = SFSpeechRecognizer(locale: Locale(identifier: "ja-JP")) else { print("no recognizer"); exit(1) }
for f in files {
    let req = SFSpeechURLRecognitionRequest(url: URL(fileURLWithPath: f))
    req.shouldReportPartialResults = false
    if rec.supportsOnDeviceRecognition { req.requiresOnDeviceRecognition = true }
    let done = DispatchSemaphore(value: 0)
    var text = ""
    rec.recognitionTask(with: req) { result, error in
        if let r = result, r.isFinal { text = r.bestTranscription.formattedString; done.signal() }
        else if error != nil { text = "(認識できず)"; done.signal() }
    }
    while done.wait(timeout: .now() + 0.05) == .timedOut { RunLoop.current.run(until: Date() + 0.05) }
    print("\((f as NSString).lastPathComponent)\t\(text)")
}
