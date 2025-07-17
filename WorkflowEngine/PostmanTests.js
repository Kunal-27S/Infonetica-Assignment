// Test script for Workflow Engine API

// Health check endpoint
pm.test("Health check returns 200 OK", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    pm.expect(response).to.have.property('message', 'Workflow Engine is running.');
});

// Workflow Definitions
pm.test("Workflow definition response has correct schema", function () {
    const response = pm.response.json();
    
    // Verify response structure
    pm.expect(response).to.be.an('object');
    pm.expect(response).to.have.property('count');
    pm.expect(response).to.have.property('definitions');
    
    // Verify count is non-negative
    pm.expect(response.count).to.be.a('number').and.to.be.at.least(0);
    
    // Verify definitions array
    pm.expect(response.definitions).to.be.an('array');
    
    // If there are definitions, validate their schema
    if (response.definitions.length > 0) {
        response.definitions.forEach(definition => {
            // Validate definition structure
            pm.expect(definition).to.have.all.keys('id', 'name', 'states', 'actions', 'description');
            
            // Validate states
            pm.expect(definition.states).to.be.an('object');
            Object.values(definition.states).forEach(state => {
                pm.expect(state).to.have.all.keys('id', 'name', 'isInitial', 'isFinal', 'enabled', 'description');
                pm.expect(state.isInitial).to.be.a('boolean');
                pm.expect(state.isFinal).to.be.a('boolean');
                pm.expect(state.enabled).to.be.a('boolean');
            });
            
            // Validate actions
            pm.expect(definition.actions).to.be.an('object');
            Object.values(definition.actions).forEach(action => {
                pm.expect(action).to.have.all.keys('id', 'name', 'enabled', 'fromStates', 'toState', 'description');
                pm.expect(action.enabled).to.be.a('boolean');
                pm.expect(action.fromStates).to.be.an('array');
                pm.expect(action.toState).to.be.a('string');
            });
        });
    }
});

// Workflow Instances
pm.test("Workflow instance response has correct schema", function () {
    const response = pm.response.json();
    
    // Verify response structure
    pm.expect(response).to.be.an('object');
    pm.expect(response).to.have.property('count');
    pm.expect(response).to.have.property('instances');
    
    // Verify count is non-negative
    pm.expect(response.count).to.be.a('number').and.to.be.at.least(0);
    
    // Verify instances array
    pm.expect(response.instances).to.be.an('array');
    
    // If there are instances, validate their schema
    if (response.instances.length > 0) {
        response.instances.forEach(instance => {
            pm.expect(instance).to.have.all.keys('id', 'definitionId', 'currentState', 'history');
            pm.expect(instance.history).to.be.an('array');
            
            // Validate history items
            instance.history.forEach(historyItem => {
                pm.expect(historyItem).to.have.all.keys('actionId', 'fromState', 'toState', 'timestamp');
                pm.expect(historyItem.timestamp).to.be.a('string');
            });
        });
    }
});

// Common Response Validation
pm.test("Response has correct headers", function () {
    pm.expect(pm.response.headers.get('Content-Type')).to.eql('application/json');
    pm.expect(pm.response.headers.get('Content-Length')).to.be.a('string');
});

pm.test("Response time is reasonable", function () {
    pm.expect(pm.response.responseTime).to.be.below(200);
});

// Error Response Validation
pm.test("Error response has correct format", function () {
    const response = pm.response.json();
    if (pm.response.code >= 400) {
        pm.expect(response).to.have.property('error').that.is.a('string');
    }
});
