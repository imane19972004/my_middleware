package fr.unice.polytech.aigenerator;

public class MockImageAnalysisService implements ImageAnalysisService {
    private AnalysisResult mockResult;

    public void setMockResult(AnalysisResult result) {
        this.mockResult = result;
    }

    @Override
    public AnalysisResult analyze(byte[] image) {
        return this.mockResult;
    }
}