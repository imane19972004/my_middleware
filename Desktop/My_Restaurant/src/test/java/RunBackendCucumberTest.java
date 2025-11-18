import org.junit.platform.suite.api.*;
import static io.cucumber.junit.platform.engine.Constants.*;

@Suite
@IncludeEngines("cucumber")
@SelectClasspathResource("features")
@ConfigurationParameter(key = GLUE_PROPERTY_NAME,
        value = "fr.unice.polytech.restaurants,fr.unice.polytech.stepDefs.back")
@ConfigurationParameter(key = PLUGIN_PROPERTY_NAME,
        value = "pretty,summary")
public class RunBackendCucumberTest {

}
