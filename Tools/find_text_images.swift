// 画像の中に英語の文字があるかを macOS の文字認識（Vision）で探します。
//   swift Tools/find_text_images.swift <画像のパス...>
// 見つかった文字と確からしさを「パス<TAB>確からしさ<TAB>文字」で出力します。
import Foundation
import Vision
import AppKit

for path in CommandLine.arguments.dropFirst() {
    guard let image = NSImage(contentsOfFile: path),
          var rect = Optional(CGRect(origin: .zero, size: image.size)),
          let cg = image.cgImage(forProposedRect: &rect, context: nil, hints: nil) else { continue }
    // pixel art is small: scale it up so the recognizer can read it
    let scale = max(1, 512 / max(cg.width, cg.height))
    var input = cg
    if scale > 1, let ctx = CGContext(data: nil, width: cg.width * scale, height: cg.height * scale, bitsPerComponent: 8,
                                     bytesPerRow: 0, space: CGColorSpaceCreateDeviceRGB(),
                                     bitmapInfo: CGImageAlphaInfo.premultipliedLast.rawValue) {
        ctx.setFillColor(CGColor(gray: 0.5, alpha: 1))
        ctx.fill(CGRect(x: 0, y: 0, width: ctx.width, height: ctx.height))
        ctx.interpolationQuality = .none
        ctx.draw(cg, in: CGRect(x: 0, y: 0, width: ctx.width, height: ctx.height))
        if let scaled = ctx.makeImage() { input = scaled }
    }
    let request = VNRecognizeTextRequest()
    request.recognitionLevel = .accurate
    request.recognitionLanguages = ["en-US"]
    request.usesLanguageCorrection = true
    try? VNImageRequestHandler(cgImage: input, options: [:]).perform([request])
    for obs in request.results ?? [] {
        guard let best = obs.topCandidates(1).first else { continue }
        let text = best.string.trimmingCharacters(in: .whitespaces)
        if text.filter({ $0.isLetter }).count >= 2 {
            print("\(path)\t\(String(format: "%.2f", best.confidence))\t\(text)")
        }
    }
}
